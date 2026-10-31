namespace PoolNet.Domain.Entities;

public class LeagueMatch
{
    public int Id { get; set; }

    public int LeagueId { get; set; }
    public League? League { get; set; }

    /// <summary>
    /// Numer kolejki ligowej
    /// </summary>
    public int RoundNumber { get; set; }

    /// <summary>
    /// Runda: 1 = Pierwszy mecz, 2 = Rewanż
    /// </summary>
    public int Leg { get; set; }

    public int MatchId { get; set; }
    public Match? Match { get; set; }
}
