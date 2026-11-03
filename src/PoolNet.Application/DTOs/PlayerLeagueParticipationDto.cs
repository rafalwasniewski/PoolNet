using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class PlayerLeagueParticipationDto
{
    public int LeagueId { get; set; }
    public string LeagueName { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public LeagueStatus Status { get; set; }
    public int Position { get; set; }
    public int TotalParticipants { get; set; }
    public int Points { get; set; }
    public int MatchesPlayed { get; set; }
}
