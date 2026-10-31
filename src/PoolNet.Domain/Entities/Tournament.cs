using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Domain.Entities;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BilliardGameType GameType { get; set; }
    public int RaceTo { get; set; }
    public TournamentStatus Status { get; set; } = TournamentStatus.InProgress;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? WinnerId { get; set; }
    public Player? Winner { get; set; }

    public ICollection<TournamentParticipant> Participants { get; set; } = new List<TournamentParticipant>();
    public ICollection<TournamentMatch> Matches { get; set; } = new List<TournamentMatch>();

    public Tournament()
    {
    }

    public Tournament(string name, BilliardGameType gameType, int raceTo)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Nazwa turnieju nie może być pusta.");
        }

        if (raceTo <= 0)
        {
            throw new DomainException("Liczba partii potrzebna do zwycięstwa (Race to) musi być większa od zera.");
        }

        Name = name.Trim();
        GameType = gameType;
        RaceTo = raceTo;
        Status = TournamentStatus.InProgress;
        CreatedAt = DateTime.UtcNow;
    }

    public void FinishTournament(int winnerId)
    {
        WinnerId = winnerId;
        Status = TournamentStatus.Finished;
    }
}
