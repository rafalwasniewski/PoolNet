using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class CreateFriendlyMatchDto
{
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public BilliardGameType GameType { get; set; } = BilliardGameType.EightBall;
    public int FirstToWins { get; set; } = 5;
}

public class RecordMatchScoreDto
{
    public int MatchId { get; set; }
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
}

public class MatchDto
{
    public int Id { get; set; }
    public BilliardGameType GameType { get; set; }
    public MatchKind MatchKind { get; set; }
    public int Player1Id { get; set; }
    public string Player1Nickname { get; set; } = string.Empty;
    public int Player2Id { get; set; }
    public string Player2Nickname { get; set; } = string.Empty;
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public int FirstToWins { get; set; }
    public int? WinnerId { get; set; }
    public string? WinnerNickname { get; set; }
    public MatchStatus Status { get; set; }
    public DateTime? PlayedAt { get; set; }
    public string ContextName { get; set; } = string.Empty;
}
