namespace PoolNet.Domain.Entities;

public class LeagueParticipant
{
    public int LeagueId { get; set; }
    public League? League { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }
}
