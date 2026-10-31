using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Domain.Entities;

public class League
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public int RaceTo { get; set; }
    public LeagueStatus Status { get; set; } = LeagueStatus.InProgress;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LeagueParticipant> Participants { get; set; } = new List<LeagueParticipant>();
    public ICollection<LeagueMatch> Matches { get; set; } = new List<LeagueMatch>();

    public League()
    {
    }

    public League(string name, BilliardGameType gameType, int raceTo)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Nazwa ligi nie może być pusta.");
        }

        if (raceTo <= 0)
        {
            throw new DomainException("Liczba partii potrzebna do zwycięstwa (Race to) musi być większa od zera.");
        }

        Name = name.Trim();
        GameType = gameType;
        RaceTo = raceTo;
        Status = LeagueStatus.InProgress;
        CreatedAt = DateTime.UtcNow;
    }

    public void CheckCompletion()
    {
        if (Matches.Count > 0 && Matches.All(m => m.Match != null && m.Match.Status == MatchStatus.Completed))
        {
            Status = LeagueStatus.Finished;
        }
    }
}
