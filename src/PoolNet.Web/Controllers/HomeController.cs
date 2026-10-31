using Microsoft.AspNetCore.Mvc;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Enums;
using PoolNet.Web.Models;

namespace PoolNet.Web.Controllers;

public class HomeController : Controller
{
    private readonly IPlayerService _playerService;
    private readonly IMatchService _matchService;
    private readonly ILeagueService _leagueService;
    private readonly ITournamentService _tournamentService;

    public HomeController(
        IPlayerService playerService,
        IMatchService matchService,
        ILeagueService leagueService,
        ITournamentService tournamentService)
    {
        _playerService = playerService;
        _matchService = matchService;
        _leagueService = leagueService;
        _tournamentService = tournamentService;
    }

    public async Task<IActionResult> Index()
    {
        var allPlayers = await _playerService.GetAllPlayersAsync();
        var recentMatches = await _matchService.GetRecentMatchesAsync(8);
        var allLeagues = await _leagueService.GetAllLeaguesAsync();
        var allTournaments = await _tournamentService.GetAllTournamentsAsync();

        var topPlayers = allPlayers
            .Where(p => p.TotalMatches > 0)
            .OrderByDescending(p => p.Wins)
            .ThenByDescending(p => p.WinRate)
            .Take(5)
            .ToList();

        var activeLeagues = allLeagues
            .Where(l => l.Status == LeagueStatus.InProgress)
            .Take(4)
            .ToList();

        var activeTournaments = allTournaments
            .Where(t => t.Status == TournamentStatus.InProgress)
            .Take(4)
            .ToList();

        var viewModel = new HomeDashboardViewModel
        {
            TopPlayers = topPlayers,
            RecentMatches = recentMatches,
            ActiveLeagues = activeLeagues,
            ActiveTournaments = activeTournaments,
            TotalPlayersCount = allPlayers.Count,
            TotalMatchesCount = recentMatches.Count
        };

        return View(viewModel);
    }
}
