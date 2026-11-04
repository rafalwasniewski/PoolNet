using Microsoft.EntityFrameworkCore;
using PoolNet.Application.Services;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;

namespace PoolNet.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Run seed only on an empty database. Never delete existing data.
        if (await context.Players.AnyAsync())
        {
            return;
        }

        var rand = new Random(42); // Fixed seed for reproducible demo data

        // 1. Add 50 players (unique nicknames only)
        var nicknames = new List<string>
        {
            "Efren Reyes", "Ronnie O'Sullivan", "Shane Van Boening", "Joshua Filler", "Francisco Bustamante",
            "Earl Strickland", "Mika Immonen", "Thorsten Hohmann", "Darren Appleton", "Niels Feijen",
            "Jayson Shaw", "Albin Ouschan", "Ko Pin-yi", "Ko Ping-chung", "Chang Jung-lin",
            "Wu Jia-qing", "Alex Pagulayan", "Ralf Souquet", "Johnny Archer", "Corey Deuel",
            "Eklent Kaci", "David Alcaide", "Francisco Sanchez Ruiz", "Fedor Gorst", "Wiktor Zielinski",
            "Wojciech Szewczyk", "Mieszko Fortunski", "Konrad Juszczyszyn", "Tomasz Kaplan", "Mateusz Sniegocki",
            "Mario He", "Alexander Kazakis", "Aloysius Yapp", "Naoyuki Oi", "Carlo Biado",
            "Dennis Orcollo", "Lee Van Corteza", "Johann Chua", "Oliver Ortmann", "Thomas Engert",
            "Nick van den Berg", "Mark Selby", "Judd Trump", "Neil Robertson", "John Higgins",
            "Shaun Murphy", "Ding Junhui", "Kyren Wilson", "Mark Allen", "Luca Brecel"
        };

        var players = new List<Player>();
        foreach (var nick in nicknames)
        {
            var p = new Player(nick);
            context.Players.Add(p);
            players.Add(p);
        }
        await context.SaveChangesAsync();

        var playerIds = players.Select(p => p.Id).ToList();

        // 2. Generate 3 billiard leagues (all 50 players participate in each)
        var leagueConfigs = new[]
        {
            ("Ekstraklasa Ósemki", BilliardGameType.EightBall, 5),
            ("Superliga Dziewiątki", BilliardGameType.NineBall, 6),
            ("Mistrzowska Liga Dziesiątki", BilliardGameType.TenBall, 6)
        };

        var scheduleGenerator = new LeagueScheduleGenerator();

        foreach (var (name, gameType, raceTo) in leagueConfigs)
        {
            var league = new League(name, gameType, raceTo);
            context.Leagues.Add(league);
            await context.SaveChangesAsync();

            // Assign all 50 players to the league
            foreach (var pid in playerIds)
            {
                context.LeagueParticipants.Add(new LeagueParticipant
                {
                    LeagueId = league.Id,
                    PlayerId = pid
                });
            }

            // Generate schedule: double round-robin home & away (50 * 49 = 2450 matches)
            var pairings = scheduleGenerator.GenerateDoubleRoundRobin(playerIds);

            var leagueMatches = new List<LeagueMatch>(pairings.Count);
            foreach (var p in pairings)
            {
                var match = new Match(gameType, MatchKind.League, p.Player1Id, p.Player2Id, raceTo);

                // Simulate league match with realistic pool score
                var (s1, s2) = GenerateScore(raceTo, rand);
                var matchDate = DateTime.UtcNow.AddDays(-rand.Next(1, 90));
                match.RecordResult(s1, s2, matchDate);

                leagueMatches.Add(new LeagueMatch
                {
                    LeagueId = league.Id,
                    RoundNumber = p.RoundNumber,
                    Leg = p.Leg,
                    Match = match
                });
            }

            context.LeagueMatches.AddRange(leagueMatches);
            league.Status = LeagueStatus.Finished;
            await context.SaveChangesAsync();
        }

        // 3. Generate 3 knockout tournaments (all 50 players participate in each)
        var tournamentConfigs = new[]
        {
            ("Puchar Polski w Ósemkę", BilliardGameType.EightBall, 5),
            ("Puchar Mistrzów w Dziewiątkę", BilliardGameType.NineBall, 6),
            ("Grand Prix Polski w Dziesiątkę", BilliardGameType.TenBall, 6)
        };

        var bracketGenerator = new TournamentBracketGenerator();

        foreach (var (name, gameType, raceTo) in tournamentConfigs)
        {
            var tournament = new Tournament(name, gameType, raceTo);
            context.Tournaments.Add(tournament);
            await context.SaveChangesAsync();

            // Seed all 50 players
            for (int i = 0; i < playerIds.Count; i++)
            {
                context.TournamentParticipants.Add(new TournamentParticipant
                {
                    TournamentId = tournament.Id,
                    PlayerId = playerIds[i],
                    Seed = i + 1
                });
            }
            await context.SaveChangesAsync();

            // Generate tournament bracket
            var bracketNodes = bracketGenerator.GenerateBracket(playerIds);

            var createdMap = new Dictionary<int, TournamentMatch>();
            foreach (var node in bracketNodes)
            {
                var tm = new TournamentMatch
                {
                    TournamentId = tournament.Id,
                    RoundNumber = node.RoundNumber,
                    MatchOrder = node.MatchOrder,
                    Player1Id = node.Player1Id,
                    Player2Id = node.Player2Id,
                    NextMatchSlot = node.NextMatchSlot,
                    IsBye = node.IsBye
                };
                context.TournamentMatches.Add(tm);
                createdMap[node.TempId] = tm;
            }
            await context.SaveChangesAsync();

            // Link next matches in the bracket
            foreach (var node in bracketNodes)
            {
                var tm = createdMap[node.TempId];
                if (node.NextTempId.HasValue && createdMap.TryGetValue(node.NextTempId.Value, out var nextNode))
                {
                    tm.NextTournamentMatchId = nextNode.Id;
                }
            }
            await context.SaveChangesAsync();

            // Simulate playing the entire tournament from round 1 to the final
            int maxRound = bracketNodes.Max(b => b.RoundNumber);

            for (int r = 1; r <= maxRound; r++)
            {
                var roundNodes = createdMap.Values.Where(m => m.RoundNumber == r).OrderBy(m => m.MatchOrder).ToList();

                foreach (var tm in roundNodes)
                {
                    if (tm.IsBye)
                    {
                        // Player with a bye advances automatically
                        int advancingPlayerId = tm.Player1Id!.Value;
                        if (tm.NextTournamentMatchId.HasValue)
                        {
                            var nextMatch = createdMap.Values.First(x => x.Id == tm.NextTournamentMatchId.Value);
                            if (tm.NextMatchSlot == 1) nextMatch.Player1Id = advancingPlayerId;
                            else nextMatch.Player2Id = advancingPlayerId;
                        }
                        continue;
                    }

                    if (tm.Player1Id.HasValue && tm.Player2Id.HasValue)
                    {
                        var match = new Match(gameType, MatchKind.Tournament, tm.Player1Id.Value, tm.Player2Id.Value, raceTo);
                        var (s1, s2) = GenerateScore(raceTo, rand);
                        var matchDate = DateTime.UtcNow.AddDays(-(maxRound - r + 1) * 2);
                        match.RecordResult(s1, s2, matchDate);

                        context.Matches.Add(match);
                        tm.Match = match;

                        int winnerId = match.WinnerId!.Value;

                        if (tm.NextTournamentMatchId.HasValue)
                        {
                            var nextMatch = createdMap.Values.First(x => x.Id == tm.NextTournamentMatchId.Value);
                            if (tm.NextMatchSlot == 1) nextMatch.Player1Id = winnerId;
                            else nextMatch.Player2Id = winnerId;
                        }
                        else
                        {
                            // Final! Determine tournament champion
                            tournament.FinishTournament(winnerId);
                        }
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }

    private static (int score1, int score2) GenerateScore(int raceTo, Random rand)
    {
        int loserScore = rand.Next(0, raceTo);
        bool p1Wins = rand.Next(2) == 0;
        return p1Wins ? (raceTo, loserScore) : (loserScore, raceTo);
    }
}
