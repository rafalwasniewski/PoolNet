namespace PoolNet.Application.DTOs;

public class LeagueRoundDto
{
    public int RoundNumber { get; set; }
    public int Leg { get; set; }
    public string Title => $"Kolejka {RoundNumber} ({(Leg == 1 ? "Runda zasadnicza" : "Rewanże")})";
    public List<LeagueMatchItemDto> Matches { get; set; } = new();
}
