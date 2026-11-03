using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class LeagueDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public int RaceTo { get; set; }
    public LeagueStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<LeagueStandingDto> Standings { get; set; } = new();
    public List<LeagueRoundDto> Rounds { get; set; } = new();
}
