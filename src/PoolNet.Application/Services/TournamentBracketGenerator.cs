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

        // Tworzymy strukturę meczów runda po rundzie
        var allNodes = new List<BracketNodeDefinition>();
        var roundsNodes = new Dictionary<int, List<BracketNodeDefinition>>();

        int currentTempId = 1;

        // Generujemy puste węzły dla każdej rundy
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

        // Łączymy mecze z następną rundą (NextTempId, NextMatchSlot)
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

        // Rozmieszczamy graczy w rundzie 1
        // Jeśli N < powerOfTwo, część graczy otrzymuje "BYE" (wolny los)
        var round1Matches = roundsNodes[1];
        int byesCount = powerOfTwo - n;

        // Przygotowujemy listę slotów dla rundy 1 (null oznacza BYE)
        var slots = new List<int?>();
        int playerIdx = 0;

        // Rozdzielamy BYE równomiernie na drugie pozycje w parach
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

        // Wypełniamy mecze rundy 1
        for (int i = 0; i < round1Matches.Count; i++)
        {
            var match = round1Matches[i];
            int? p1 = slots[2 * i];
            int? p2 = slots[2 * i + 1];

            match.Player1Id = p1;
            match.Player2Id = p2;

            if (p1.HasValue && !p2.HasValue)
            {
                // Wolny los dla gracza 1 -> automatyczny awans do kolejnej rundy
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
