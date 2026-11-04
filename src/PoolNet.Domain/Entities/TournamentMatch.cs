namespace PoolNet.Domain.Entities;

public class TournamentMatch
{
    public int Id { get; set; }

    public int TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    /// <summary>
    /// Round number (e.g., 1 = quarterfinals, 2 = semifinals, 3 = final, etc.)
    /// </summary>
    public int RoundNumber { get; set; }

    /// <summary>
    /// Match position in the given round
    /// </summary>
    public int MatchOrder { get; set; }

    public int? MatchId { get; set; }
    public Match? Match { get; set; }

    public int? Player1Id { get; set; }
    public Player? Player1 { get; set; }

    public int? Player2Id { get; set; }
    public Player? Player2 { get; set; }

    /// <summary>
    /// Next match in the bracket that the winner advances to
    /// </summary>
    public int? NextTournamentMatchId { get; set; }
    public TournamentMatch? NextTournamentMatch { get; set; }

    /// <summary>
    /// 1 = Player1 in the next match, 2 = Player2 in the next match
    /// </summary>
    public int? NextMatchSlot { get; set; }

    /// <summary>
    /// Flag indicating a bye (walkover)
    /// </summary>
    public bool IsBye { get; set; }
}
