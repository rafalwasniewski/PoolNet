namespace PoolNet.Application.DTOs;

public class TournamentMatchNodeDto
{
    public int TournamentMatchId { get; set; }
    public int MatchOrder { get; set; }
    public int? MatchId { get; set; }
    public int? Player1Id { get; set; }
    public string? Player1Nickname { get; set; }
    public int? Player2Id { get; set; }
    public string? Player2Nickname { get; set; }
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public int? WinnerId { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsBye { get; set; }
    public bool CanEdit { get; set; }
    public int? NextTournamentMatchId { get; set; }
}
