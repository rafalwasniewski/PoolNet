using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class PlayerTournamentParticipationDto
{
    public int TournamentId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public TournamentStatus Status { get; set; }
    public string Result { get; set; } = string.Empty;
}
