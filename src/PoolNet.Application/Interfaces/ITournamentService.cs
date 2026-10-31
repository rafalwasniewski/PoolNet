using PoolNet.Application.DTOs;

namespace PoolNet.Application.Interfaces;

public interface ITournamentService
{
    Task<TournamentDetailsDto> CreateTournamentAsync(CreateTournamentDto dto, CancellationToken cancellationToken = default);
    Task<List<TournamentSummaryDto>> GetAllTournamentsAsync(CancellationToken cancellationToken = default);
    Task<TournamentDetailsDto?> GetTournamentDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<TournamentDetailsDto> RecordTournamentMatchScoreAsync(int tournamentMatchId, int player1Score, int player2Score, CancellationToken cancellationToken = default);
}
