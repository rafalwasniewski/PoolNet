using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

public class CreateTournamentDto
{
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; } = BilliardGameType.EightBall;
    public int RaceTo { get; set; } = 5;
    public List<int> ParticipantPlayerIds { get; set; } = new();
}

public class TournamentSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public int RaceTo { get; set; }
    public TournamentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ParticipantsCount { get; set; }
    public string? WinnerNickname { get; set; }
}

public class TournamentDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public int RaceTo { get; set; }
    public TournamentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? WinnerId { get; set; }
    public string? WinnerNickname { get; set; }

    public List<TournamentParticipantDto> Participants { get; set; } = new();
    public List<TournamentRoundDto> Rounds { get; set; } = new();
}

public class TournamentParticipantDto
{
    public int PlayerId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Seed { get; set; }
}

public class TournamentRoundDto
{
    public int RoundNumber { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public List<TournamentMatchNodeDto> Matches { get; set; } = new();
}

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
    public int? NextTournamentMatchId { get; set; }
}
