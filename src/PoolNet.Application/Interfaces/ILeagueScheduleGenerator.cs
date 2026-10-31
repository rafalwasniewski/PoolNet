namespace PoolNet.Application.Interfaces;

public record ScheduledPairing(int RoundNumber, int Leg, int Player1Id, int Player2Id);

public interface ILeagueScheduleGenerator
{
    /// <summary>
    /// Generuje terminarz w systemie kołowym (mecz i rewanż - każdy z każdym 2 razy)
    /// </summary>
    List<ScheduledPairing> GenerateDoubleRoundRobin(IReadOnlyList<int> playerIds);
}
