namespace PoolNet.Application.DTOs;

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
