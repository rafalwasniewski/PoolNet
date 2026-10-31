using PoolNet.Application.DTOs;

namespace PoolNet.Application.Interfaces;

public interface IMatchService
{
    Task<MatchDto> CreateFriendlyMatchAsync(CreateFriendlyMatchDto dto, CancellationToken cancellationToken = default);
    Task<MatchDto> RecordScoreAsync(RecordMatchScoreDto dto, CancellationToken cancellationToken = default);
    Task<MatchDto?> GetMatchByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<MatchDto>> GetRecentMatchesAsync(int count = 10, CancellationToken cancellationToken = default);
}
