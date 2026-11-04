using Microsoft.EntityFrameworkCore;
using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Application.Services;

public class PlayerService : IPlayerService
{
    private readonly IAppDbContext _db;
    private readonly IPlayerStatisticsCalculator _statisticsCalculator;
    private readonly ILeagueStandingsCalculator _standingsCalculator;

    public PlayerService(
        IAppDbContext db,
        IPlayerStatisticsCalculator statisticsCalculator,
        ILeagueStandingsCalculator standingsCalculator)
    {
        _db = db;
        _statisticsCalculator = statisticsCalculator;
        _standingsCalculator = standingsCalculator;
    }

    public async Task<PlayerSummaryDto> CreatePlayerAsync(CreatePlayerDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nickname))
        {
            throw new DomainException("Pseudonim gracza nie może być pusty.");
        }

        string trimmedNickname = dto.Nickname.Trim();

        bool exists = await _db.Players.AnyAsync(p => p.Nickname.ToLower() == trimmedNickname.ToLower(), cancellationToken);
        if (exists)
        {
            throw new DomainException($"Gracz o pseudonimie '{trimmedNickname}' już istnieje w systemie.");
        }

        var player = new Player(trimmedNickname);
        _db.Players.Add(player);
        await _db.SaveChangesAsync(cancellationToken);

        return new PlayerSummaryDto
        {
            Id = player.Id,
            Nickname = player.Nickname,
            CreatedAt = player.CreatedAt,
            TotalMatches = 0,
            Wins = 0,
            Losses = 0,
            WinRate = 0
        };
    }

    public async Task<List<PlayerSummaryDto>> GetAllPlayersAsync(CancellationToken cancellationToken = default)
    {
        var players = await _db.Players
            .AsNoTracking()
            .OrderBy(p => p.Nickname)
            .ToListAsync(cancellationToken);

        var playerIds = players.Select(p => p.Id).ToList();

        var completedMatches = await _db.Matches
            .AsNoTracking()
            .Where(m => m.Status == MatchStatus.Completed && (playerIds.Contains(m.Player1Id) || playerIds.Contains(m.Player2Id)))
            .Select(m => new { m.Player1Id, m.Player2Id, m.WinnerId })
            .ToListAsync(cancellationToken);

        var result = new List<PlayerSummaryDto>();

        foreach (var player in players)
        {
            var pMatches = completedMatches.Where(m => m.Player1Id == player.Id || m.Player2Id == player.Id).ToList();
            int wins = pMatches.Count(m => m.WinnerId == player.Id);
            int total = pMatches.Count;
            double winRate = total > 0 ? Math.Round((double)wins / total * 100, 1) : 0;

            result.Add(new PlayerSummaryDto
            {
                Id = player.Id,
                Nickname = player.Nickname,
                CreatedAt = player.CreatedAt,
                TotalMatches = total,
                Wins = wins,
                Losses = total - wins,
                WinRate = winRate
            });
        }

        return result;
    }

    public async Task<PlayerDetailsDto?> GetPlayerDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var player = await _db.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (player == null)
        {
            return null;
        }

        var matches = await _db.Matches
            .AsNoTracking()
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Where(m => m.Player1Id == id || m.Player2Id == id)
            .OrderByDescending(m => m.PlayedAt ?? DateTime.MinValue)
            .ToListAsync(cancellationToken);

        // Find match associations with leagues and tournaments
        var matchIds = matches.Select(m => m.Id).ToList();

        var leagueMatches = await _db.LeagueMatches
            .AsNoTracking()
            .Include(lm => lm.League)
            .Where(lm => matchIds.Contains(lm.MatchId))
            .ToDictionaryAsync(lm => lm.MatchId, lm => lm.League?.Name ?? "Liga", cancellationToken);

        var tournamentMatches = await _db.TournamentMatches
            .AsNoTracking()
            .Include(tm => tm.Tournament)
            .Where(tm => tm.MatchId.HasValue && matchIds.Contains(tm.MatchId.Value))
            .ToDictionaryAsync(tm => tm.MatchId!.Value, tm => tm.Tournament?.Name ?? "Turniej", cancellationToken);

        var matchHistory = new List<PlayerMatchHistoryDto>();
        foreach (var m in matches)
        {
            bool isPlayer1 = m.Player1Id == id;
            var opp = isPlayer1 ? m.Player2 : m.Player1;
            int oppId = isPlayer1 ? m.Player2Id : m.Player1Id;
            int pScore = isPlayer1 ? m.Player1Score : m.Player2Score;
            int oppScore = isPlayer1 ? m.Player2Score : m.Player1Score;

            string contextName = "Mecz towarzyski";
            if (m.MatchKind == MatchKind.League && leagueMatches.TryGetValue(m.Id, out var lName))
            {
                contextName = $"Liga: {lName}";
            }
            else if (m.MatchKind == MatchKind.Tournament && tournamentMatches.TryGetValue(m.Id, out var tName))
            {
                contextName = $"Turniej: {tName}";
            }

            matchHistory.Add(new PlayerMatchHistoryDto
            {
                MatchId = m.Id,
                PlayedAt = m.PlayedAt,
                GameType = m.GameType,
                MatchKind = m.MatchKind,
                ContextName = contextName,
                OpponentId = oppId,
                OpponentNickname = opp?.Nickname ?? $"Gracz #{oppId}",
                PlayerScore = pScore,
                OpponentScore = oppScore,
                FirstToWins = m.FirstToWins,
                IsWinner = m.WinnerId == id
            });
        }

        var overallStats = _statisticsCalculator.CalculateOverall(id, matches);
        var statsPerGameType = _statisticsCalculator.CalculatePerGameType(id, matches);

        // Player leagues
        var playerLeagues = await _db.Leagues
            .AsNoTracking()
            .AsSplitQuery()
            .Include(l => l.Matches)
                .ThenInclude(lm => lm.Match)
            .Include(l => l.Participants)
                .ThenInclude(p => p.Player)
            .Where(l => l.Participants.Any(p => p.PlayerId == id))
            .ToListAsync(cancellationToken);

        var leaguesDto = new List<PlayerLeagueParticipationDto>();
        foreach (var league in playerLeagues)
        {
            var allMatches = league.Matches.Select(m => m.Match).Where(m => m != null)!;
            var standings = _standingsCalculator.CalculateStandings(league.Participants, allMatches!);
            var myStanding = standings.FirstOrDefault(s => s.PlayerId == id);

            leaguesDto.Add(new PlayerLeagueParticipationDto
            {
                LeagueId = league.Id,
                LeagueName = league.Name,
                GameType = league.GameType,
                Status = league.Status,
                Position = myStanding?.Position ?? 1,
                TotalParticipants = league.Participants.Count,
                Points = myStanding?.Points ?? 0,
                MatchesPlayed = myStanding?.MatchesPlayed ?? 0
            });
        }

        // Player tournaments
        var playerTournaments = await _db.Tournaments
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Matches)
                .ThenInclude(tm => tm.Match)
            .Include(t => t.Participants)
            .Where(t => t.Participants.Any(p => p.PlayerId == id))
            .ToListAsync(cancellationToken);

        var tournamentsDto = new List<PlayerTournamentParticipationDto>();
        foreach (var tournament in playerTournaments)
        {
            string resultText = "W trakcie";
            if (tournament.WinnerId == id)
            {
                resultText = "🏆 Zwycięzca (Mistrz)";
            }
            else if (tournament.Status == TournamentStatus.Finished)
            {
                // Check which round the player was eliminated in
                var myLoss = tournament.Matches
                    .Where(tm => (tm.Player1Id == id || tm.Player2Id == id) && tm.Match != null && tm.Match.Status == MatchStatus.Completed && tm.Match.WinnerId != id)
                    .OrderByDescending(tm => tm.RoundNumber)
                    .FirstOrDefault();

                if (myLoss != null)
                {
                    int maxRound = tournament.Matches.Max(m => m.RoundNumber);
                    if (myLoss.RoundNumber == maxRound)
                    {
                        resultText = "🥈 Finał (2. miejsce)";
                    }
                    else if (myLoss.RoundNumber == maxRound - 1)
                    {
                        resultText = "🥉 Półfinał";
                    }
                    else if (myLoss.RoundNumber == maxRound - 2)
                    {
                        resultText = "Ćwierćfinał";
                    }
                    else
                    {
                        resultText = $"Runda {myLoss.RoundNumber}";
                    }
                }
                else
                {
                    resultText = "Uczestnik";
                }
            }

            tournamentsDto.Add(new PlayerTournamentParticipationDto
            {
                TournamentId = tournament.Id,
                TournamentName = tournament.Name,
                GameType = tournament.GameType,
                Status = tournament.Status,
                Result = resultText
            });
        }

        return new PlayerDetailsDto
        {
            Id = player.Id,
            Nickname = player.Nickname,
            CreatedAt = player.CreatedAt,
            OverallStats = overallStats,
            StatsPerGameType = statsPerGameType,
            MatchHistory = matchHistory,
            Leagues = leaguesDto,
            Tournaments = tournamentsDto
        };
    }

    public async Task<bool> PlayerExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Players.AnyAsync(p => p.Id == id, cancellationToken);
    }
}
