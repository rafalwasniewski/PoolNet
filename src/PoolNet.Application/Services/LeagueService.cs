using Microsoft.EntityFrameworkCore;
using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Application.Services;

public class LeagueService : ILeagueService
{
    private readonly IAppDbContext _db;
    private readonly ILeagueScheduleGenerator _scheduleGenerator;
    private readonly ILeagueStandingsCalculator _standingsCalculator;

    public LeagueService(
        IAppDbContext db,
        ILeagueScheduleGenerator scheduleGenerator,
        ILeagueStandingsCalculator standingsCalculator)
    {
        _db = db;
        _scheduleGenerator = scheduleGenerator;
        _standingsCalculator = standingsCalculator;
    }

    public async Task<LeagueDetailsDto> CreateLeagueAsync(CreateLeagueDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new DomainException("Nazwa ligi nie może być pusta.");
        }

        if (dto.ParticipantPlayerIds == null || dto.ParticipantPlayerIds.Count < 2)
        {
            throw new DomainException("Liga musi liczyć co najmniej 2 graczy.");
        }

        if (dto.RaceTo <= 0)
        {
            throw new DomainException("Liczba wygranych partii (Race to) musi być większa od zera.");
        }

        var players = await _db.Players
            .Where(p => dto.ParticipantPlayerIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (players.Count != dto.ParticipantPlayerIds.Distinct().Count())
        {
            throw new DomainException("Jeden lub więcej wybranych graczy nie istnieje w systemie.");
        }

        var league = new League(dto.Name, dto.GameType, dto.RaceTo);
        _db.Leagues.Add(league);
        await _db.SaveChangesAsync(cancellationToken);

        // Add participants
        foreach (var playerId in dto.ParticipantPlayerIds.Distinct())
        {
            _db.LeagueParticipants.Add(new LeagueParticipant
            {
                LeagueId = league.Id,
                PlayerId = playerId
            });
        }

        // Generate double round-robin schedule (home & away)
        var pairings = _scheduleGenerator.GenerateDoubleRoundRobin(dto.ParticipantPlayerIds.Distinct().ToList());

        foreach (var pairing in pairings)
        {
            var match = new Match(
                dto.GameType,
                MatchKind.League,
                pairing.Player1Id,
                pairing.Player2Id,
                dto.RaceTo
            );

            _db.LeagueMatches.Add(new LeagueMatch
            {
                LeagueId = league.Id,
                RoundNumber = pairing.RoundNumber,
                Leg = pairing.Leg,
                Match = match
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return (await GetLeagueDetailsAsync(league.Id, cancellationToken))!;
    }

    public async Task<List<LeagueSummaryDto>> GetAllLeaguesAsync(CancellationToken cancellationToken = default)
    {
        // Aggregations computed at database level to avoid loading all Match entities into memory.
        return await _db.Leagues
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LeagueSummaryDto
            {
                Id = l.Id,
                Name = l.Name,
                GameType = l.GameType,
                RaceTo = l.RaceTo,
                Status = l.Status,
                CreatedAt = l.CreatedAt,
                ParticipantsCount = l.Participants.Count,
                TotalMatchesCount = l.Matches.Count,
                CompletedMatchesCount = l.Matches.Count(m =>
                    m.Match != null && m.Match.Status == MatchStatus.Completed)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<LeagueDetailsDto?> GetLeagueDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var league = await _db.Leagues
            .AsNoTracking()
            .AsSplitQuery()
            .Include(l => l.Participants)
                .ThenInclude(p => p.Player)
            .Include(l => l.Matches)
                .ThenInclude(lm => lm.Match)
                    .ThenInclude(m => m!.Player1)
            .Include(l => l.Matches)
                .ThenInclude(lm => lm.Match)
                    .ThenInclude(m => m!.Player2)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (league == null) return null;

        var allMatches = league.Matches.Select(m => m.Match).Where(m => m != null)!;
        var standings = _standingsCalculator.CalculateStandings(league.Participants, allMatches!);

        // Group matches by round and leg
        var rounds = league.Matches
            .GroupBy(m => new { m.RoundNumber, m.Leg })
            .OrderBy(g => g.Key.RoundNumber)
            .Select(g => new LeagueRoundDto
            {
                RoundNumber = g.Key.RoundNumber,
                Leg = g.Key.Leg,
                Matches = g.Select(lm => new LeagueMatchItemDto
                {
                    LeagueMatchId = lm.Id,
                    MatchId = lm.MatchId,
                    Player1Id = lm.Match?.Player1Id ?? 0,
                    Player1Nickname = lm.Match?.Player1?.Nickname ?? string.Empty,
                    Player2Id = lm.Match?.Player2Id ?? 0,
                    Player2Nickname = lm.Match?.Player2?.Nickname ?? string.Empty,
                    Player1Score = lm.Match?.Player1Score ?? 0,
                    Player2Score = lm.Match?.Player2Score ?? 0,
                    WinnerId = lm.Match?.WinnerId,
                    Status = lm.Match?.Status ?? MatchStatus.Scheduled
                }).ToList()
            }).ToList();

        return new LeagueDetailsDto
        {
            Id = league.Id,
            Name = league.Name,
            GameType = league.GameType,
            RaceTo = league.RaceTo,
            Status = league.Status,
            CreatedAt = league.CreatedAt,
            Standings = standings,
            Rounds = rounds
        };
    }

    public async Task<LeagueDetailsDto> RecordLeagueMatchScoreAsync(int leagueMatchId, int player1Score, int player2Score, CancellationToken cancellationToken = default)
    {
        var leagueMatch = await _db.LeagueMatches
            .Include(lm => lm.Match)
            .Include(lm => lm.League)
                .ThenInclude(l => l!.Matches)
                    .ThenInclude(m => m.Match)
            .FirstOrDefaultAsync(lm => lm.Id == leagueMatchId, cancellationToken);

        if (leagueMatch == null || leagueMatch.Match == null || leagueMatch.League == null)
        {
            throw new DomainException($"Nie znaleziono meczu ligowego o ID {leagueMatchId}.");
        }

        leagueMatch.Match.RecordResult(player1Score, player2Score);

        leagueMatch.League.CheckCompletion();

        await _db.SaveChangesAsync(cancellationToken);

        return (await GetLeagueDetailsAsync(leagueMatch.LeagueId, cancellationToken))!;
    }
}
