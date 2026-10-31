using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoolNet.Application.DTOs;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Exceptions;
using PoolNet.Web.Models;

namespace PoolNet.Web.Controllers;

public class PlayersController : Controller
{
    private readonly IPlayerService _playerService;

    public PlayersController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search = null)
    {
        var players = await _playerService.GetAllPlayersAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            players = players.Where(p => p.Nickname.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Search = search;
        }

        return View(players);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var playerDetails = await _playerService.GetPlayerDetailsAsync(id);
        if (playerDetails == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono wskazanego gracza.";
            return RedirectToAction(nameof(Index));
        }

        return View(playerDetails);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new CreatePlayerViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePlayerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var created = await _playerService.CreatePlayerAsync(new CreatePlayerDto
            {
                Nickname = model.Nickname
            });

            TempData["SuccessMessage"] = $"Pomyślnie dodano nowego gracza '{created.Nickname}'.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(nameof(model.Nickname), ex.Message);
            return View(model);
        }
    }
}
