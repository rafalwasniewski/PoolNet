using Microsoft.EntityFrameworkCore;
using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Application.Services;

public class TournamentService : ITournamentService
{
    private readonly IAppDbContext _db;
    private readonly ITournamentBracketGenerator _bracketGenerator;

    public TournamentService(IAppDbContext db, ITournamentBracketGenerator bracketGenerator)
    {
        _db = db;
        _bracketGenerator = bracketGenerator;
    }

    public async Task<TournamentDetailsDto> CreateTournamentAsync(CreateTournamentDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new DomainException("Nazwa turnieju nie może być pusta.");
        }

        if (dto.ParticipantPlayerIds == null || dto.ParticipantPlayerIds.Count < 2)
        {
            throw new DomainException("Turniej pucharowy musi liczyć co najmniej 2 uczestników.");
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

        var tournament = new Tournament(dto.Name, dto.GameType, dto.RaceTo);
        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync(cancellationToken);

        // Dodaj uczestników z rozstawieniem
        var participantIds = dto.ParticipantPlayerIds.Distinct().ToList();
        for (int i = 0; i < participantIds.Count; i++)
        {
            _db.TournamentParticipants.Add(new TournamentParticipant
            {
                TournamentId = tournament.Id,
                PlayerId = participantIds[i],
                Seed = i + 1
            });
        }
        await _db.SaveChangesAsync(cancellationToken);

        // Wygeneruj strukturę drabinki
        var bracketNodes = _bracketGenerator.GenerateBracket(participantIds);

        // Słownik do mapowania TempId na utworzone encje TournamentMatch
        var createdEntities = new Dictionary<int, TournamentMatch>();

        // Krok 1: Tworzenie wszystkich encji TournamentMatch
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

            _db.TournamentMatches.Add(tm);
            createdEntities[node.TempId] = tm;
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Krok 2: Powiązanie NextTournamentMatchId oraz utworzenie meczów (Match) dla gotowych par w rundzie 1 (lub kolejnych jeśli bye)
        foreach (var node in bracketNodes)
        {
            var tm = createdEntities[node.TempId];
            if (node.NextTempId.HasValue && createdEntities.TryGetValue(node.NextTempId.Value, out var nextEntity))
            {
                tm.NextTournamentMatchId = nextEntity.Id;
            }

            // Jeśli mecz ma obu graczy i nie jest wolnym losem, stwórz mecz bilardowy
            if (tm.Player1Id.HasValue && tm.Player2Id.HasValue && !tm.IsBye && tm.MatchId == null)
            {
                var match = new Match(
                    tournament.GameType,
                    MatchKind.Tournament,
                    tm.Player1Id.Value,
                    tm.Player2Id.Value,
                    tournament.RaceTo
                );
                tm.Match = match;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        return (await GetTournamentDetailsAsync(tournament.Id, cancellationToken))!;
    }

    public async Task<List<TournamentSummaryDto>> GetAllTournamentsAsync(CancellationToken cancellationToken = default)
    {
        var tournaments = await _db.Tournaments
            .AsNoTracking()
            .Include(t => t.Participants)
            .Include(t => t.Winner)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tournaments.Select(t => new TournamentSummaryDto
        {
            Id = t.Id,
            Name = t.Name,
            GameType = t.GameType,
            RaceTo = t.RaceTo,
            Status = t.Status,
            CreatedAt = t.CreatedAt,
            ParticipantsCount = t.Participants.Count,
            WinnerNickname = t.Winner?.Nickname
        }).ToList();
    }

    public async Task<TournamentDetailsDto?> GetTournamentDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var tournament = await _db.Tournaments
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Winner)
            .Include(t => t.Participants)
                .ThenInclude(p => p.Player)
            .Include(t => t.Matches)
                .ThenInclude(tm => tm.Player1)
            .Include(t => t.Matches)
                .ThenInclude(tm => tm.Player2)
            .Include(t => t.Matches)
                .ThenInclude(tm => tm.Match)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tournament == null) return null;

        int totalRounds = tournament.Matches.Any() ? tournament.Matches.Max(m => m.RoundNumber) : 1;

        var participantsDto = tournament.Participants
            .OrderBy(p => p.Seed)
            .Select(p => new TournamentParticipantDto
            {
                PlayerId = p.PlayerId,
                Nickname = p.Player?.Nickname ?? $"Gracz #{p.PlayerId}",
                Seed = p.Seed
            }).ToList();

        var roundsDto = tournament.Matches
            .GroupBy(m => m.RoundNumber)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                int r = g.Key;
                string roundName = GetRoundDisplayName(r, totalRounds);

                return new TournamentRoundDto
                {
                    RoundNumber = r,
                    RoundName = roundName,
                    Matches = g.OrderBy(m => m.MatchOrder).Select(tm => new TournamentMatchNodeDto
                    {
                        TournamentMatchId = tm.Id,
                        MatchOrder = tm.MatchOrder,
                        MatchId = tm.MatchId,
                        Player1Id = tm.Player1Id,
                        Player1Nickname = tm.Player1?.Nickname,
                        Player2Id = tm.Player2Id,
                        Player2Nickname = tm.Player2?.Nickname,
                        Player1Score = tm.Match?.Player1Score ?? 0,
                        Player2Score = tm.Match?.Player2Score ?? 0,
                        WinnerId = tm.Match?.WinnerId,
                        IsCompleted = tm.Match?.Status == MatchStatus.Completed,
                        IsBye = tm.IsBye,
                        NextTournamentMatchId = tm.NextTournamentMatchId,
                        CanEdit = !tm.IsBye
                            && tm.Player1Id.HasValue
                            && tm.Player2Id.HasValue
                            && !(tm.Match?.Status == MatchStatus.Completed
                                && tm.NextTournamentMatchId.HasValue
                                && tournament.Matches.Any(next =>
                                    next.Id == tm.NextTournamentMatchId.Value
                                    && next.Match?.Status == MatchStatus.Completed))
                    }).ToList()
                };
            }).ToList();

        return new TournamentDetailsDto
        {
            Id = tournament.Id,
            Name = tournament.Name,
            GameType = tournament.GameType,
            RaceTo = tournament.RaceTo,
            Status = tournament.Status,
            CreatedAt = tournament.CreatedAt,
            WinnerId = tournament.WinnerId,
            WinnerNickname = tournament.Winner?.Nickname,
            Participants = participantsDto,
            Rounds = roundsDto
        };
    }

    public async Task<TournamentDetailsDto> RecordTournamentMatchScoreAsync(int tournamentMatchId, int player1Score, int player2Score, CancellationToken cancellationToken = default)
    {
        var tm = await _db.TournamentMatches
            .Include(m => m.Tournament)
            .Include(m => m.Match)
            .Include(m => m.NextTournamentMatch)
                .ThenInclude(n => n!.Match)
            .FirstOrDefaultAsync(m => m.Id == tournamentMatchId, cancellationToken);

        if (tm == null || tm.Tournament == null)
        {
            throw new DomainException($"Nie znaleziono meczu turniejowego o ID {tournamentMatchId}.");
        }

        if (tm.Match == null)
        {
            if (!tm.Player1Id.HasValue || !tm.Player2Id.HasValue)
            {
                throw new DomainException("Mecz nie ma jeszcze skompletowanych obu graczy.");
            }

            // Utwórz mecz jeśli nie istniał
            var newMatch = new Match(
                tm.Tournament.GameType,
                MatchKind.Tournament,
                tm.Player1Id.Value,
                tm.Player2Id.Value,
                tm.Tournament.RaceTo
            );
            _db.Matches.Add(newMatch);
            await _db.SaveChangesAsync(cancellationToken);
            tm.MatchId = newMatch.Id;
            tm.Match = newMatch;
        }

        // Jeśli zwycięzca tego meczu rozegrał już kolejną rundę, zmiana wyniku wymagałaby
        // wycofania kolejnych rezultatów. Zamiast doprowadzać do niespójności — blokujemy edycję.
        if (tm.NextTournamentMatch?.Match?.Status == MatchStatus.Completed)
        {
            throw new DomainException(
                "Nie można zmienić wyniku meczu, ponieważ jego zwycięzca rozegrał już kolejną rundę turnieju.");
        }

        tm.Match.RecordResult(player1Score, player2Score);
        int winnerId = tm.Match.WinnerId!.Value;

        // Awans zwycięzcy do kolejnej rundy lub zakończenie turnieju
        if (tm.NextTournamentMatchId.HasValue)
        {
            var nextTm = tm.NextTournamentMatch;
            if (nextTm == null)
            {
                nextTm = await _db.TournamentMatches
                    .Include(m => m.Match)
                    .FirstOrDefaultAsync(m => m.Id == tm.NextTournamentMatchId.Value, cancellationToken);
            }

            if (nextTm != null)
            {
                if (tm.NextMatchSlot == 1)
                {
                    nextTm.Player1Id = winnerId;
                }
                else
                {
                    nextTm.Player2Id = winnerId;
                }

                // Utrzymujemy zgodność encji Match z węzłem drabinki także przy poprawianiu wyniku.
                if (nextTm.Match != null)
                {
                    if (tm.NextMatchSlot == 1)
                    {
                        nextTm.Match.Player1Id = winnerId;
                    }
                    else
                    {
                        nextTm.Match.Player2Id = winnerId;
                    }
                }
                // Jeśli w następnym meczu obaj gracze są już znani i mecz nie ma jeszcze przypisanego Match, utwórz go
                else if (nextTm.Player1Id.HasValue && nextTm.Player2Id.HasValue && nextTm.MatchId == null)
                {
                    var subsequentMatch = new Match(
                        tm.Tournament.GameType,
                        MatchKind.Tournament,
                        nextTm.Player1Id.Value,
                        nextTm.Player2Id.Value,
                        tm.Tournament.RaceTo
                    );
                    _db.Matches.Add(subsequentMatch);
                    await _db.SaveChangesAsync(cancellationToken);
                    nextTm.MatchId = subsequentMatch.Id;
                    nextTm.Match = subsequentMatch;
                }
            }
        }
        else
        {
            // Brak kolejnego meczu = był to FINAŁ!
            tm.Tournament.FinishTournament(winnerId);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return (await GetTournamentDetailsAsync(tm.TournamentId, cancellationToken))!;
    }

    private static string GetRoundDisplayName(int round, int totalRounds)
    {
        int distanceFromFinal = totalRounds - round;
        return distanceFromFinal switch
        {
            0 => "Finał",
            1 => "Półfinały",
            2 => "Ćwierćfinały",
            3 => "1/8 finału",
            4 => "1/16 finału",
            _ => $"Runda {round}"
        };
    }
}
