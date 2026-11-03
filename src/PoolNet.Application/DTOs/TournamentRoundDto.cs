namespace PoolNet.Application.DTOs;

public class TournamentRoundDto
{
    public int RoundNumber { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public List<TournamentMatchNodeDto> Matches { get; set; } = new();
}
