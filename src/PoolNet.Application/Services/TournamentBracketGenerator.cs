using PoolNet.Application.Interfaces;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Application.Services;

public class TournamentBracketGenerator : ITournamentBracketGenerator
{
    public List<BracketNodeDefinition> GenerateBracket(IReadOnlyList<int> playerIds)
    {
        if (playerIds == null || playerIds.Count < 2)
        {
            throw new DomainException("Turniej musi liczyć co najmniej 2 uczestników.");
        }

        var uniquePlayers = playerIds.Distinct().ToList();
        if (uniquePlayers.Count != playerIds.Count)
        {
            throw new DomainException("Lista uczestników turnieju zawiera duplikaty.");
        }

        int n = uniquePlayers.Count;
        int powerOfTwo = 2;
        while (powerOfTwo < n)
        {
            powerOfTwo *= 2;
        }

        int totalRounds = (int)Math.Log2(powerOfTwo);

        // Build the match structure round by round
        var allNodes = new List<BracketNodeDefinition>();
        var roundsNodes = new Dictionary<int, List<BracketNodeDefinition>>();

        int currentTempId = 1;

        // Generate placeholder nodes for each round
        for (int r = 1; r <= totalRounds; r++)
        {
            int matchesInRound = powerOfTwo / (1 << r);
            roundsNodes[r] = new List<BracketNodeDefinition>();

            for (int m = 0; m < matchesInRound; m++)
            {
                var node = new BracketNodeDefinition
                {
                    TempId = currentTempId++,
                    RoundNumber = r,
                    MatchOrder = m
                };
                roundsNodes[r].Add(node);
                allNodes.Add(node);
            }
        }

        // Link matches to the next round (NextTempId, NextMatchSlot)
        for (int r = 1; r < totalRounds; r++)
        {
            var currentRoundMatches = roundsNodes[r];
            var nextRoundMatches = roundsNodes[r + 1];

            for (int m = 0; m < currentRoundMatches.Count; m++)
            {
                var node = currentRoundMatches[m];
                int nextMatchOrder = m / 2;
                var targetNode = nextRoundMatches[nextMatchOrder];

                node.NextTempId = targetNode.TempId;
                node.NextMatchSlot = (m % 2 == 0) ? 1 : 2;
            }
        }

        // Seed players into round 1
        // If N < powerOfTwo, remaining slots receive a "BYE" (walkover)
        var round1Matches = roundsNodes[1];
        int byesCount = powerOfTwo - n;

        // Prepare the list of slots for round 1 (null means BYE)
        var slots = new List<int?>();
        int playerIdx = 0;

        // Distribute BYEs evenly across secondary positions in pairs
        for (int i = 0; i < round1Matches.Count; i++)
        {
            slots.Add(uniquePlayers[playerIdx++]);

            if (byesCount > 0 && playerIdx + (round1Matches.Count - 1 - i) >= n)
            {
                slots.Add(null); // BYE
                byesCount--;
            }
            else
            {
                slots.Add(playerIdx < n ? uniquePlayers[playerIdx++] : null);
            }
        }

        // Populate round 1 matches
        for (int i = 0; i < round1Matches.Count; i++)
        {
            var match = round1Matches[i];
            int? p1 = slots[2 * i];
            int? p2 = slots[2 * i + 1];

            match.Player1Id = p1;
            match.Player2Id = p2;

            if (p1.HasValue && !p2.HasValue)
            {
                // Bye for player 1 -> automatic advance to the next round
                match.IsBye = true;
                if (match.NextTempId.HasValue)
                {
                    var nextNode = allNodes.First(x => x.TempId == match.NextTempId.Value);
                    if (match.NextMatchSlot == 1)
                    {
                        nextNode.Player1Id = p1;
                    }
                    else
                    {
                        nextNode.Player2Id = p1;
                    }
                }
            }
        }

        return allNodes;
    }
}
