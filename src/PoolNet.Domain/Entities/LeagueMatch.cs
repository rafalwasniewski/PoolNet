namespace PoolNet.Domain.Entities;

public class LeagueMatch
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public League? League { get; set; }

    /// <summary>
    /// League round/fixture number
    /// </summary>
    public int RoundNumber { get; set; }

    /// <summary>
    /// Leg: 1 = First leg, 2 = Rematch / Return leg
    /// </summary>
    public int Leg { get; set; }

    public int MatchId { get; set; }
    public Match? Match { get; set; }
}
