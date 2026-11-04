namespace PoolNet.Application.Interfaces;

public record ScheduledPairing(int RoundNumber, int Leg, int Player1Id, int Player2Id);

public interface ILeagueScheduleGenerator
{
    /// <summary>
    /// Generates a double round-robin schedule (home and away - each player plays every other player twice)
    /// </summary>
    List<ScheduledPairing> GenerateDoubleRoundRobin(IReadOnlyList<int> playerIds);
}
