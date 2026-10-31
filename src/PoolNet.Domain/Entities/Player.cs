using PoolNet.Domain.Exceptions;

namespace PoolNet.Domain.Entities;

public class Player
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Match> MatchesAsPlayer1 { get; set; } = new List<Match>();
    public ICollection<Match> MatchesAsPlayer2 { get; set; } = new List<Match>();
    public ICollection<TournamentParticipant> TournamentParticipations { get; set; } = new List<TournamentParticipant>();
    public ICollection<LeagueParticipant> LeagueParticipations { get; set; } = new List<LeagueParticipant>();

    public Player()
    {
    }

    public Player(string nickname)
    {
        SetNickname(nickname);
        CreatedAt = DateTime.UtcNow;
    }

    public void SetNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            throw new DomainException("Pseudonim gracza nie może być pusty.");
        }

        Nickname = nickname.Trim();
    }
}
