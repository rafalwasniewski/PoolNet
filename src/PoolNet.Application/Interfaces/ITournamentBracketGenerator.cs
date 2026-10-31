using PoolNet.Domain.Entities;

namespace PoolNet.Application.Interfaces;

public class BracketNodeDefinition
{
    public int TempId { get; set; }
    public int RoundNumber { get; set; }
    public int MatchOrder { get; set; }
    public int? Player1Id { get; set; }
    public int? Player2Id { get; set; }
    public bool IsBye { get; set; }
    public int? NextTempId { get; set; }
    public int? NextMatchSlot { get; set; }
}

public interface ITournamentBracketGenerator
{
    List<BracketNodeDefinition> GenerateBracket(IReadOnlyList<int> playerIds);
}
