using PoolNet.Application.DTOs;

namespace PoolNet.Application.Interfaces;

public interface IPlayerService
{
    Task<PlayerSummaryDto> CreatePlayerAsync(CreatePlayerDto dto, CancellationToken cancellationToken = default);
    Task<List<PlayerSummaryDto>> GetAllPlayersAsync(CancellationToken cancellationToken = default);
    Task<PlayerDetailsDto?> GetPlayerDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> PlayerExistsAsync(int id, CancellationToken cancellationToken = default);
}
