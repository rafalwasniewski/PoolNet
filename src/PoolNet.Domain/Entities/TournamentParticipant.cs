namespace PoolNet.Domain.Entities;

public class TournamentParticipant
{
    public int TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    public int Seed { get; set; }
}
