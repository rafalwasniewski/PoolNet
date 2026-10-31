using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class CreatePlayerDto
{
    public string Nickname { get; set; } = string.Empty;
}

public class PlayerSummaryDto
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalMatches { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate { get; set; }
}

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

public class OverallStatisticsDto
{
    public int TotalMatches { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate { get; set; }
    public int FramesWon { get; set; }
    public int FramesLost { get; set; }
    public int FrameDifference => FramesWon - FramesLost;
}

public class GameTypeStatisticsDto
{
    public BilliardGameType GameType { get; set; }
    public string GameTypeName => GameType switch
    {
        BilliardGameType.EightBall => "Bila 8 (Ósemka)",
        BilliardGameType.NineBall => "Bila 9 (Dziewiątka)",
        BilliardGameType.TenBall => "Bila 10 (Dziesiątka)",
        _ => GameType.ToString()
    };
    public int TotalMatches { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate => TotalMatches > 0 ? Math.Round((double)Wins / TotalMatches * 100, 1) : 0;
    public int FramesWon { get; set; }
    public int FramesLost { get; set; }
    public int FrameDifference => FramesWon - FramesLost;
}

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

public class PlayerTournamentParticipationDto
{
    public int TournamentId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public TournamentStatus Status { get; set; }
    public string Result { get; set; } = string.Empty;
}
