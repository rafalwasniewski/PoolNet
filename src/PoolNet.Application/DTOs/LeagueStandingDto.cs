namespace PoolNet.Application.DTOs;

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
