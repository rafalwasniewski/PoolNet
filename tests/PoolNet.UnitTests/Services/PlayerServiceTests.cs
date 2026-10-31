using FluentAssertions;
using PoolNet.Application.DTOs;
using PoolNet.Application.Services;
using PoolNet.Domain.Exceptions;
using PoolNet.UnitTests.Helpers;
using Xunit;

namespace PoolNet.UnitTests.Services;

public class PlayerServiceTests : IDisposable
{
    private readonly PoolNet.Infrastructure.Data.AppDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly PlayerService _playerService;

    public PlayerServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.CreateInMemoryDbContext();
        var statsCalc = new PlayerStatisticsCalculator();
        _playerService = new PlayerService(_context, statsCalc);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreatePlayerAsync_WithUniqueNickname_CreatesAndReturnsPlayer()
    {
        // Act
        var result = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "SnookerKing" });

        // Assert
        result.Should().NotBeNull();
        result.Nickname.Should().Be("SnookerKing");
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreatePlayerAsync_WithDuplicateNickname_ThrowsDomainException()
    {
        // Arrange
        await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "TheMagician" });

        // Act
        var act = async () => await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "themagician" });

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*już istnieje w systemie*");
    }

    [Fact]
    public async Task GetAllPlayersAsync_ReturnsAllAddedPlayers()
    {
        // Arrange
        await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "PlayerA" });
        await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "PlayerB" });

        // Act
        var players = await _playerService.GetAllPlayersAsync();

        // Assert
        players.Should().HaveCount(2);
        players.Select(p => p.Nickname).Should().Contain(new[] { "PlayerA", "PlayerB" });
    }
}
