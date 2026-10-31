using Microsoft.EntityFrameworkCore;
using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Application.Services;

public class MatchService : IMatchService
{
    private readonly IAppDbContext _db;

    public MatchService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<MatchDto> CreateFriendlyMatchAsync(CreateFriendlyMatchDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Player1Id == dto.Player2Id)
        {
            throw new DomainException("Gracz nie może grać przeciwko samemu sobie.");
        }

        var player1 = await _db.Players.FindAsync([dto.Player1Id], cancellationToken);
        var player2 = await _db.Players.FindAsync([dto.Player2Id], cancellationToken);

        if (player1 == null || player2 == null)
        {
            throw new DomainException("Jeden lub obaj wskazani gracze nie istnieją.");
        }

        if (dto.FirstToWins <= 0)
        {
            throw new DomainException("Limit wygranych partii (Race to) musi być większy od zera.");
        }

        var match = new Match(dto.GameType, MatchKind.Friendly, dto.Player1Id, dto.Player2Id, dto.FirstToWins);
        _db.Matches.Add(match);
        await _db.SaveChangesAsync(cancellationToken);

        return new MatchDto
        {
            Id = match.Id,
            GameType = match.GameType,
            MatchKind = match.MatchKind,
            Player1Id = match.Player1Id,
            Player1Nickname = player1.Nickname,
            Player2Id = match.Player2Id,
            Player2Nickname = player2.Nickname,
            Player1Score = match.Player1Score,
            Player2Score = match.Player2Score,
            FirstToWins = match.FirstToWins,
            Status = match.Status,
            PlayedAt = match.PlayedAt,
            ContextName = "Mecz towarzyski"
        };
    }

    public async Task<MatchDto> RecordScoreAsync(RecordMatchScoreDto dto, CancellationToken cancellationToken = default)
    {
        var match = await _db.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .FirstOrDefaultAsync(m => m.Id == dto.MatchId, cancellationToken);

        if (match == null)
        {
            throw new DomainException($"Nie znaleziono meczu o ID {dto.MatchId}.");
        }

        match.RecordResult(dto.Player1Score, dto.Player2Score);
        await _db.SaveChangesAsync(cancellationToken);

        string winnerNickname = match.WinnerId == match.Player1Id
            ? match.Player1?.Nickname ?? string.Empty
            : match.Player2?.Nickname ?? string.Empty;

        return new MatchDto
        {
            Id = match.Id,
            GameType = match.GameType,
            MatchKind = match.MatchKind,
            Player1Id = match.Player1Id,
            Player1Nickname = match.Player1?.Nickname ?? string.Empty,
            Player2Id = match.Player2Id,
            Player2Nickname = match.Player2?.Nickname ?? string.Empty,
            Player1Score = match.Player1Score,
            Player2Score = match.Player2Score,
            FirstToWins = match.FirstToWins,
            WinnerId = match.WinnerId,
            WinnerNickname = winnerNickname,
            Status = match.Status,
            PlayedAt = match.PlayedAt,
            ContextName = match.MatchKind switch
            {
                MatchKind.Friendly => "Mecz towarzyski",
                MatchKind.League => "Mecz ligowy",
                MatchKind.Tournament => "Mecz turniejowy",
                _ => "Mecz bilardowy"
            }
        };
    }

    public async Task<MatchDto?> GetMatchByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var match = await _db.Matches
            .AsNoTracking()
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (match == null) return null;

        string? winnerNick = match.WinnerId == match.Player1Id ? match.Player1?.Nickname : match.Player2?.Nickname;

        return new MatchDto
        {
            Id = match.Id,
            GameType = match.GameType,
            MatchKind = match.MatchKind,
            Player1Id = match.Player1Id,
            Player1Nickname = match.Player1?.Nickname ?? string.Empty,
            Player2Id = match.Player2Id,
            Player2Nickname = match.Player2?.Nickname ?? string.Empty,
            Player1Score = match.Player1Score,
            Player2Score = match.Player2Score,
            FirstToWins = match.FirstToWins,
            WinnerId = match.WinnerId,
            WinnerNickname = winnerNick,
            Status = match.Status,
            PlayedAt = match.PlayedAt,
            ContextName = match.MatchKind switch
            {
                MatchKind.Friendly => "Mecz towarzyski",
                MatchKind.League => "Mecz ligowy",
                MatchKind.Tournament => "Mecz turniejowy",
                _ => "Mecz"
            }
        };
    }

    public async Task<List<MatchDto>> GetRecentMatchesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var matches = await _db.Matches
            .AsNoTracking()
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .OrderByDescending(m => m.PlayedAt ?? DateTime.MinValue)
            .ThenByDescending(m => m.Id)
            .Take(count)
            .ToListAsync(cancellationToken);

        return matches.Select(m => new MatchDto
        {
            Id = m.Id,
            GameType = m.GameType,
            MatchKind = m.MatchKind,
            Player1Id = m.Player1Id,
            Player1Nickname = m.Player1?.Nickname ?? string.Empty,
            Player2Id = m.Player2Id,
            Player2Nickname = m.Player2?.Nickname ?? string.Empty,
            Player1Score = m.Player1Score,
            Player2Score = m.Player2Score,
            FirstToWins = m.FirstToWins,
            WinnerId = m.WinnerId,
            WinnerNickname = m.WinnerId == m.Player1Id ? m.Player1?.Nickname : m.Player2?.Nickname,
            Status = m.Status,
            PlayedAt = m.PlayedAt,
            ContextName = m.MatchKind switch
            {
                MatchKind.Friendly => "Mecz towarzyski",
                MatchKind.League => "Mecz ligowy",
                MatchKind.Tournament => "Mecz turniejowy",
                _ => "Mecz"
            }
        }).ToList();
    }
}
