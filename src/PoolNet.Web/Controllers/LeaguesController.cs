using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Exceptions;
using PoolNet.Web.Models;

namespace PoolNet.Web.Controllers;

public class LeaguesController : Controller
{
    private readonly ILeagueService _leagueService;
    private readonly IPlayerService _playerService;

    public LeaguesController(ILeagueService leagueService, IPlayerService playerService)
    {
        _leagueService = leagueService;
        _playerService = playerService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var leagues = await _leagueService.GetAllLeaguesAsync();
        return View(leagues);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var league = await _leagueService.GetLeagueDetailsAsync(id);
        if (league == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono wybranej ligi.";
            return RedirectToAction(nameof(Index));
        }

        return View(league);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var players = await _playerService.GetAllPlayersAsync();
        var model = new CreateLeagueViewModel
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
    public async Task<IActionResult> Create(CreateLeagueViewModel model)
    {
        if (model.SelectedPlayerIds == null || model.SelectedPlayerIds.Count < 2)
        {
            ModelState.AddModelError(nameof(model.SelectedPlayerIds), "Do utworzenia ligi wymagany jest wybór co najmniej 2 graczy.");
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
            var league = await _leagueService.CreateLeagueAsync(new CreateLeagueDto
            {
                Name = model.Name,
                GameType = model.GameType,
                RaceTo = model.RaceTo,
                ParticipantPlayerIds = model.SelectedPlayerIds!
            });

            TempData["SuccessMessage"] = $"Liga '{league.Name}' z terminarzem mecz i rewanż została pomyślnie utworzona!";
            return RedirectToAction(nameof(Details), new { id = league.Id });
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
    public async Task<IActionResult> RecordMatchScore(int leagueId, int leagueMatchId, int player1Score, int player2Score)
    {
        try
        {
            var league = await _leagueService.RecordLeagueMatchScoreAsync(leagueMatchId, player1Score, player2Score);
            TempData["SuccessMessage"] = "Wynik meczu ligowego został zapisany i tabela zaktualizowana.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = leagueId });
    }
}
