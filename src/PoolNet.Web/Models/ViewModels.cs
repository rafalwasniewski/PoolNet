using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PoolNet.Application.DTOs;
using PoolNet.Domain.Enums;

namespace PoolNet.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Pole Login jest wymagane.")]
    [Display(Name = "Login administratora")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pole Hasło jest wymagane.")]
    [DataType(DataType.Password)]
    [Display(Name = "Hasło")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class CreatePlayerViewModel
{
    [Required(ErrorMessage = "Pseudonim gracza jest wymagany.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Pseudonim musi mieć od 2 do 50 znaków.")]
    [Display(Name = "Pseudonim gracza (Nickname)")]
    public string Nickname { get; set; } = string.Empty;
}

public class CreateFriendlyMatchViewModel
{
    [Required(ErrorMessage = "Wybierz gracza 1.")]
    [Display(Name = "Gracz 1")]
    public int Player1Id { get; set; }

    [Required(ErrorMessage = "Wybierz gracza 2.")]
    [Display(Name = "Gracz 2")]
    public int Player2Id { get; set; }

    [Required]
    [EnumDataType(typeof(BilliardGameType), ErrorMessage = "Wybierz prawidłową odmianę bilarda.")]
    [Display(Name = "Odmiana bilarda")]
    public BilliardGameType GameType { get; set; } = BilliardGameType.EightBall;

    [Required]
    [Range(1, 50, ErrorMessage = "Liczba partii musi wynosić od 1 do 50.")]
    [Display(Name = "Do ilu wygranych partii (Race to)")]
    public int FirstToWins { get; set; } = 5;

    public List<SelectListItem> AvailablePlayers { get; set; } = new();
}

public class RecordMatchScoreViewModel
{
    public int MatchId { get; set; }
    public string Player1Nickname { get; set; } = string.Empty;
    public string Player2Nickname { get; set; } = string.Empty;

    [Range(0, 50, ErrorMessage = "Wynik partii nie może być ujemny.")]
    public int Player1Score { get; set; }

    [Range(0, 50, ErrorMessage = "Wynik partii nie może być ujemny.")]
    public int Player2Score { get; set; }

    public int FirstToWins { get; set; }
    public string? ReturnUrl { get; set; }
}

public class CreateLeagueViewModel
{
    [Required(ErrorMessage = "Nazwa ligi jest wymagana.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nazwa ligi musi mieć od 3 do 100 znaków.")]
    [Display(Name = "Nazwa ligi")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(BilliardGameType), ErrorMessage = "Wybierz prawidłową odmianę bilarda.")]
    [Display(Name = "Odmiana bilarda")]
    public BilliardGameType GameType { get; set; } = BilliardGameType.EightBall;

    [Required]
    [Range(1, 50, ErrorMessage = "Liczba partii musi wynosić od 1 do 50.")]
    [Display(Name = "Partie do wygrania w meczu (Race to)")]
    public int RaceTo { get; set; } = 5;

    [Display(Name = "Uczestnicy ligi")]
    public List<int> SelectedPlayerIds { get; set; } = new();

    public List<SelectListItem> AvailablePlayers { get; set; } = new();
}

public class CreateTournamentViewModel
{
    [Required(ErrorMessage = "Nazwa turnieju jest wymagana.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nazwa turnieju musi mieć od 3 do 100 znaków.")]
    [Display(Name = "Nazwa turnieju")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(BilliardGameType), ErrorMessage = "Wybierz prawidłową odmianę bilarda.")]
    [Display(Name = "Odmiana bilarda")]
    public BilliardGameType GameType { get; set; } = BilliardGameType.EightBall;

    [Required]
    [Range(1, 50, ErrorMessage = "Liczba partii musi wynosić od 1 do 50.")]
    [Display(Name = "Partie do wygrania w meczu (Race to)")]
    public int RaceTo { get; set; } = 5;

    [Display(Name = "Uczestnicy turnieju (kolejność wyznacza rozstawienie)")]
    public List<int> SelectedPlayerIds { get; set; } = new();

    public List<SelectListItem> AvailablePlayers { get; set; } = new();
}

public class HomeDashboardViewModel
{
    public List<PlayerSummaryDto> TopPlayers { get; set; } = new();
    public List<MatchDto> RecentMatches { get; set; } = new();
    public List<LeagueSummaryDto> ActiveLeagues { get; set; } = new();
    public List<TournamentSummaryDto> ActiveTournaments { get; set; } = new();
    public int TotalPlayersCount { get; set; }
    public int TotalMatchesCount { get; set; }
}
