using FluentAssertions;
using PoolNet.Application.DTOs;
using PoolNet.Application.Services;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;
using PoolNet.UnitTests.Helpers;
using Xunit;

namespace PoolNet.UnitTests.Services;

public class MatchServiceTests : IDisposable
{
    private readonly PoolNet.Infrastructure.Data.AppDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly MatchService _matchService;

    public MatchServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.CreateInMemoryDbContext();
        _matchService = new MatchService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task RecordScoreAsync_ForLeagueMatch_ThrowsDomainException()
    {
        // Arrange
        var (player1, player2) = await AddPlayersAsync();
        var match = new Match(BilliardGameType.EightBall, MatchKind.League, player1.Id, player2.Id, 5);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        // Act
        var act = async () => await _matchService.RecordScoreAsync(new RecordMatchScoreDto
        {
            MatchId = match.Id,
            Player1Score = 5,
            Player2Score = 2
        });

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*widoku odpowiedniej ligi lub turnieju*");
    }

    [Fact]
    public async Task RecordScoreAsync_ForFriendlyMatch_RecordsResult()
    {
        // Arrange
        var (player1, player2) = await AddPlayersAsync();
        var match = new Match(BilliardGameType.NineBall, MatchKind.Friendly, player1.Id, player2.Id, 5);
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        // Act
        var result = await _matchService.RecordScoreAsync(new RecordMatchScoreDto
        {
            MatchId = match.Id,
            Player1Score = 5,
            Player2Score = 3
        });

        // Assert
        result.Status.Should().Be(MatchStatus.Completed);
        result.WinnerId.Should().Be(player1.Id);
        result.Player1Score.Should().Be(5);
        result.Player2Score.Should().Be(3);
    }

    private async Task<(Player Player1, Player Player2)> AddPlayersAsync()
    {
        var player1 = new Player("Player 1");
        var player2 = new Player("Player 2");
        _context.Players.AddRange(player1, player2);
        await _context.SaveChangesAsync();
        return (player1, player2);
    }
}
