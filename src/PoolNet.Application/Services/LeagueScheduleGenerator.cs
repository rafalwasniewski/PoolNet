using PoolNet.Application.Interfaces;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Application.Services;

public class LeagueScheduleGenerator : ILeagueScheduleGenerator
{
    public List<ScheduledPairing> GenerateDoubleRoundRobin(IReadOnlyList<int> playerIds)
    {
        if (playerIds == null || playerIds.Count < 2)
        {
            throw new DomainException("Liga musi liczyć co najmniej 2 uczestników.");
        }

        var uniquePlayers = playerIds.Distinct().ToList();
        if (uniquePlayers.Count != playerIds.Count)
        {
            throw new DomainException("Lista uczestników ligi zawiera duplikaty.");
        }

        bool hasDummy = uniquePlayers.Count % 2 != 0;
        var participants = new List<int>(uniquePlayers);
        if (hasDummy)
        {
            participants.Add(-1); // -1 represents a bye (BYE)
        }

        int totalSlots = participants.Count;
        int halfSize = totalSlots / 2;
        int roundsPerLeg = totalSlots - 1;

        var leg1Pairings = new List<ScheduledPairing>();

        // Use the polygon rotation algorithm (Round-Robin Polygon / Berger tables)
        var rotatingList = new List<int>(participants);

        for (int round = 1; round <= roundsPerLeg; round++)
        {
            for (int i = 0; i < halfSize; i++)
            {
                int p1 = rotatingList[i];
                int p2 = rotatingList[totalSlots - 1 - i];

                if (p1 != -1 && p2 != -1)
                {
                    // Alternate home/away assignment for balanced pairings
                    bool flip = (i + round) % 2 == 0;
                    int home = flip ? p1 : p2;
                    int away = flip ? p2 : p1;

                    leg1Pairings.Add(new ScheduledPairing(round, 1, home, away));
                }
            }

            // Rotate elements (position 0 remains fixed, others shift cyclically)
            int last = rotatingList[^1];
            rotatingList.RemoveAt(rotatingList.Count - 1);
            rotatingList.Insert(1, last);
        }

        // Leg 2: Return matches with reversed home/away sides
        var leg2Pairings = new List<ScheduledPairing>();
        foreach (var p in leg1Pairings)
        {
            leg2Pairings.Add(new ScheduledPairing(
                RoundNumber: p.RoundNumber + roundsPerLeg,
                Leg: 2,
                Player1Id: p.Player2Id, // Swap sides for the return match
                Player2Id: p.Player1Id
            ));
        }

        var result = new List<ScheduledPairing>(leg1Pairings.Count + leg2Pairings.Count);
        result.AddRange(leg1Pairings);
        result.AddRange(leg2Pairings);

        return result;
    }
}
