using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Exceptions;
using PoolNet.Web.Models;

namespace PoolNet.Web.Controllers;

public class TournamentsController : Controller
{
    private readonly ITournamentService _tournamentService;
    private readonly IPlayerService _playerService;

    public TournamentsController(ITournamentService tournamentService, IPlayerService playerService)
    {
        _tournamentService = tournamentService;
        _playerService = playerService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var tournaments = await _tournamentService.GetAllTournamentsAsync();
        return View(tournaments);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var tournament = await _tournamentService.GetTournamentDetailsAsync(id);
        if (tournament == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono turnieju.";
            return RedirectToAction(nameof(Index));
        }

        return View(tournament);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var players = await _playerService.GetAllPlayersAsync();
        var model = new CreateTournamentViewModel
        {
            AvailablePlayers = players.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Nickname
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTournamentViewModel model)
    {
        if (model.SelectedPlayerIds == null || model.SelectedPlayerIds.Count < 2)
        {
            ModelState.AddModelError(nameof(model.SelectedPlayerIds), "Do utworzenia turnieju wymagany jest wybór co najmniej 2 graczy.");
        }

        if (!ModelState.IsValid)
        {
            var players = await _playerService.GetAllPlayersAsync();
            model.AvailablePlayers = players.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Nickname
            }).ToList();
            return View(model);
        }

        try
        {
            var tournament = await _tournamentService.CreateTournamentAsync(new CreateTournamentDto
            {
                Name = model.Name,
                GameType = model.GameType,
                RaceTo = model.RaceTo,
                ParticipantPlayerIds = model.SelectedPlayerIds!
            });

            TempData["SuccessMessage"] = $"Turniej pucharowy '{tournament.Name}' został pomyślnie wygenerowany!";
            return RedirectToAction(nameof(Details), new { id = tournament.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var players = await _playerService.GetAllPlayersAsync();
            model.AvailablePlayers = players.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Nickname
            }).ToList();
            return View(model);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordMatchScore(int tournamentId, int tournamentMatchId, int player1Score, int player2Score)
    {
        try
        {
            var tournament = await _tournamentService.RecordTournamentMatchScoreAsync(tournamentMatchId, player1Score, player2Score);
            TempData["SuccessMessage"] = "Wynik meczu turniejowego został zapisany. Zwycięzca awansował do kolejnej rundy!";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = tournamentId });
    }
}
