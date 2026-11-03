using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class PlayerMatchHistoryDto
{
    public int MatchId { get; set; }
    public DateTime? PlayedAt { get; set; }
    public BilliardGameType GameType { get; set; }
    public MatchKind MatchKind { get; set; }
    public string ContextName { get; set; } = string.Empty;
    public int OpponentId { get; set; }
    public string OpponentNickname { get; set; } = string.Empty;
    public int PlayerScore { get; set; }
    public int OpponentScore { get; set; }
    public int FirstToWins { get; set; }
    public bool IsWinner { get; set; }
}
