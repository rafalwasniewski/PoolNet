using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class CreateFriendlyMatchDto
{
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public BilliardGameType GameType { get; set; } = BilliardGameType.EightBall;
    public int FirstToWins { get; set; } = 5;
}
