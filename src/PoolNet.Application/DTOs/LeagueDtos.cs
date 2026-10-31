using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class CreateLeagueDto
{
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; } = BilliardGameType.EightBall;
    public int RaceTo { get; set; } = 5;
    public List<int> ParticipantPlayerIds { get; set; } = new();
}

public class LeagueSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public int RaceTo { get; set; }
    public LeagueStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ParticipantsCount { get; set; }
    public int TotalMatchesCount { get; set; }
    public int CompletedMatchesCount { get; set; }
}

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

public class LeagueStandingDto
{
    public int Position { get; set; }
    public int PlayerId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int FramesWon { get; set; }
    public int FramesLost { get; set; }
    public int FrameDifference => FramesWon - FramesLost;
    public int Points { get; set; }
}

public class LeagueRoundDto
{
    public int RoundNumber { get; set; }
    public int Leg { get; set; }
    public string Title => $"Kolejka {RoundNumber} ({(Leg == 1 ? "Runda zasadnicza" : "Rewanże")})";
    public List<LeagueMatchItemDto> Matches { get; set; } = new();
}

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
