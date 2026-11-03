namespace PoolNet.Application.DTOs;

public class TournamentParticipantDto
{
    public int PlayerId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Seed { get; set; }
}
