using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class TournamentSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public int RaceTo { get; set; }
    public TournamentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ParticipantsCount { get; set; }
    public string? WinnerNickname { get; set; }
}
