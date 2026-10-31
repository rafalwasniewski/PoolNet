namespace PoolNet.Domain.Entities;

public class TournamentMatch
{
    public int Id { get; set; }

    public int TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    /// <summary>
    /// Numer rundy (np. 1 = 1/4 finału, 2 = 1/2 finału, 3 = finał itp.)
    /// </summary>
    public int RoundNumber { get; set; }

    /// <summary>
    /// Pozycja meczu w danej rundzie
    /// </summary>
    public int MatchOrder { get; set; }

    public int? MatchId { get; set; }
    public Match? Match { get; set; }

    public int? Player1Id { get; set; }
    public Player? Player1 { get; set; }

    public int? Player2Id { get; set; }
    public Player? Player2 { get; set; }

    /// <summary>
    /// Kolejny mecz w drabince, do którego awansuje zwycięzca
    /// </summary>
    public int? NextTournamentMatchId { get; set; }
    public TournamentMatch? NextTournamentMatch { get; set; }

    /// <summary>
    /// 1 = Player1 w następnym meczu, 2 = Player2 w następnym meczu
    /// </summary>
    public int? NextMatchSlot { get; set; }

    /// <summary>
    /// Flaga oznaczająca wolny los (walkower / bye)
    /// </summary>
    public bool IsBye { get; set; }
}
