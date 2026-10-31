using PoolNet.Application.DTOs;
using PoolNet.Domain.Entities;

namespace PoolNet.Application.Interfaces;

public interface IPlayerStatisticsCalculator
{
    OverallStatisticsDto CalculateOverall(int playerId, IEnumerable<Match> matches);
    List<GameTypeStatisticsDto> CalculatePerGameType(int playerId, IEnumerable<Match> matches);
}
