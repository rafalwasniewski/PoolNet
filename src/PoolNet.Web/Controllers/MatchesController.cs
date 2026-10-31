using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Exceptions;
using PoolNet.Web.Models;

namespace PoolNet.Web.Controllers;

public class MatchesController : Controller
{
    private readonly IMatchService _matchService;
    private readonly IPlayerService _playerService;

    public MatchesController(IMatchService matchService, IPlayerService playerService)
    {
        _matchService = matchService;
        _playerService = playerService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var matches = await _matchService.GetRecentMatchesAsync(50);
        return View(matches);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateFriendly()
    {
        var players = await _playerService.GetAllPlayersAsync();
        var model = new CreateFriendlyMatchViewModel
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
    public async Task<IActionResult> CreateFriendly(CreateFriendlyMatchViewModel model)
    {
        if (model.Player1Id == model.Player2Id)
        {
            ModelState.AddModelError(string.Empty, "Gracz 1 i Gracz 2 muszą być różnymi osobami.");
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
            var match = await _matchService.CreateFriendlyMatchAsync(new CreateFriendlyMatchDto
            {
                Player1Id = model.Player1Id,
                Player2Id = model.Player2Id,
                GameType = model.GameType,
                FirstToWins = model.FirstToWins
            });

            TempData["SuccessMessage"] = "Mecz towarzyski został pomyślnie utworzony.";
            return RedirectToAction(nameof(RecordScore), new { id = match.Id });
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

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RecordScore(int id, string? returnUrl = null)
    {
        var match = await _matchService.GetMatchByIdAsync(id);
        if (match == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono meczu.";
            return RedirectToAction(nameof(Index));
        }

        var model = new RecordMatchScoreViewModel
        {
            MatchId = match.Id,
            Player1Nickname = match.Player1Nickname,
            Player2Nickname = match.Player2Nickname,
            Player1Score = match.Player1Score,
            Player2Score = match.Player2Score,
            FirstToWins = match.FirstToWins,
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordScore(RecordMatchScoreViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updated = await _matchService.RecordScoreAsync(new RecordMatchScoreDto
            {
                MatchId = model.MatchId,
                Player1Score = model.Player1Score,
                Player2Score = model.Player2Score
            });

            TempData["SuccessMessage"] = $"Wynik meczu ({updated.Player1Nickname} {updated.Player1Score} : {updated.Player2Score} {updated.Player2Nickname}) został zapisany!";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
