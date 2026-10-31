using PoolNet.Application.DTOs;

namespace PoolNet.Application.Interfaces;

public interface ILeagueService
{
    Task<LeagueDetailsDto> CreateLeagueAsync(CreateLeagueDto dto, CancellationToken cancellationToken = default);
    Task<List<LeagueSummaryDto>> GetAllLeaguesAsync(CancellationToken cancellationToken = default);
    Task<LeagueDetailsDto?> GetLeagueDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<LeagueDetailsDto> RecordLeagueMatchScoreAsync(int leagueMatchId, int player1Score, int player2Score, CancellationToken cancellationToken = default);
}
