using PoolNet.Domain.Enums;

namespace PoolNet.Application.DTOs;

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
