namespace PoolNet.Application.DTOs;

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
