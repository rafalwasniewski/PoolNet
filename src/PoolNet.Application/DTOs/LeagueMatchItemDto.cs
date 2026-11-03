using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class LeagueMatchItemDto
{
    public int LeagueMatchId { get; set; }
    public int MatchId { get; set; }
    public int Player1Id { get; set; }
    public string Player1Nickname { get; set; } = string.Empty;
    public int Player2Id { get; set; }
    public string Player2Nickname { get; set; } = string.Empty;
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public int? WinnerId { get; set; }
    public MatchStatus Status { get; set; }
}
