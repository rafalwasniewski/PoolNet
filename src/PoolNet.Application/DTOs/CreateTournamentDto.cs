using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class CreateTournamentDto
{
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; } = BilliardGameType.EightBall;
    public int RaceTo { get; set; } = 5;
    public List<int> ParticipantPlayerIds { get; set; } = new();
}
