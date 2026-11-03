using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;

namespace PoolNet.Domain.Entities;

public class Match
{
    public int Id { get; set; }
    public BilliardGameType GameType { get; set; }
    public MatchKind MatchKind { get; set; }

    public int Player1Id { get; set; }
    public Player? Player1 { get; set; }

    public int Player2Id { get; set; }
    public Player? Player2 { get; set; }

    public int Player1Score { get; set; }
    public int Player2Score { get; set; }

    /// <summary>
    /// Do ilu wygranych partii toczy się mecz (Race to)
    /// </summary>
    public int FirstToWins { get; set; }

    public int? WinnerId { get; set; }
    public Player? Winner { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public DateTime? PlayedAt { get; set; }

    public Match()
    {
    }

    public Match(
        BilliardGameType gameType,
        MatchKind matchKind,
        int player1Id,
        int player2Id,
        int firstToWins)
    {
        if (player1Id <= 0 || player2Id <= 0)
        {
            throw new DomainException("Identyfikatory graczy muszą być większe od zera.");
        }

        if (player1Id == player2Id)
        {
            throw new DomainException("Gracz nie może rozegrać meczu z samym sobą.");
        }

        if (firstToWins <= 0)
        {
            throw new DomainException("Liczba partii potrzebna do zwycięstwa (Race to) musi być większa od zera.");
        }

        GameType = gameType;
        MatchKind = matchKind;
        Player1Id = player1Id;
        Player2Id = player2Id;
        FirstToWins = firstToWins;
        Status = MatchStatus.Scheduled;
    }

    public void RecordResult(int player1Score, int player2Score, DateTime? playedAt = null)
    {
        if (player1Score < 0 || player2Score < 0)
        {
            throw new DomainException("Wynik partii nie może być ujemny.");
        }

        if (FirstToWins <= 0)
        {
            throw new DomainException("Limit partii do zwycięstwa jest nieprawidłowy.");
        }

        if (player1Score > FirstToWins || player2Score > FirstToWins)
        {
            throw new DomainException($"Żaden gracz nie może wygrać więcej partii niż ustalone {FirstToWins}.");
        }

        bool player1Won = player1Score == FirstToWins;
        bool player2Won = player2Score == FirstToWins;

        if (player1Won && player2Won)
        {
            throw new DomainException("W bilardzie nie ma remisów – obaj gracze nie mogą osiągnąć maksymalnej liczby partii.");
        }

        if (!player1Won && !player2Won)
        {
            throw new DomainException($"Zwycięzca musi zdobyć dokładnie {FirstToWins} wygranych partii.");
        }

        Player1Score = player1Score;
        Player2Score = player2Score;
        WinnerId = player1Won ? Player1Id : Player2Id;
        Status = MatchStatus.Completed;
        PlayedAt = playedAt ?? PlayedAt ?? DateTime.UtcNow;
    }
}
