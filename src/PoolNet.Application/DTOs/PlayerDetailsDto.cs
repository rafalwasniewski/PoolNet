namespace PoolNet.Application.DTOs;

public class PlayerDetailsDto
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public OverallStatisticsDto OverallStats { get; set; } = new();
    public List<GameTypeStatisticsDto> StatsPerGameType { get; set; } = new();
    public List<PlayerMatchHistoryDto> MatchHistory { get; set; } = new();
    public List<PlayerLeagueParticipationDto> Leagues { get; set; } = new();
    public List<PlayerTournamentParticipationDto> Tournaments { get; set; } = new();
}
