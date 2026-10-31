using FluentAssertions;
using PoolNet.Application.DTOs;
using PoolNet.Application.Services;
using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;
using PoolNet.UnitTests.Helpers;
using Xunit;

namespace PoolNet.UnitTests.Services;

public class LeagueServiceTests : IDisposable
{
    private readonly PoolNet.Infrastructure.Data.AppDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly LeagueService _leagueService;
    private readonly PlayerService _playerService;

    public LeagueServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.CreateInMemoryDbContext();
        var scheduleGen = new LeagueScheduleGenerator();
        _leagueService = new LeagueService(_context, scheduleGen);
        _playerService = new PlayerService(_context, new PlayerStatisticsCalculator());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateLeagueAsync_With3Players_GeneratesDoubleRoundRobinMatches()
    {
        // Arrange
        var p1 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "Gracz1" });
        var p2 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "Gracz2" });
        var p3 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "Gracz3" });

        var createDto = new CreateLeagueDto
        {
            Name = "Ekstraklasa Bila 8",
            GameType = BilliardGameType.EightBall,
            RaceTo = 5,
            ParticipantPlayerIds = new List<int> { p1.Id, p2.Id, p3.Id }
        };

        // Act
        var league = await _leagueService.CreateLeagueAsync(createDto);

        // Assert
        league.Should().NotBeNull();
        league.Name.Should().Be("Ekstraklasa Bila 8");
        league.Standings.Should().HaveCount(3);

        // 3 graczy => 3 * 2 = 6 meczów
        int totalMatches = league.Rounds.SelectMany(r => r.Matches).Count();
        totalMatches.Should().Be(6);
    }

    [Fact]
    public async Task RecordLeagueMatchScoreAsync_UpdatesStandingsAccurately()
    {
        // Arrange
        var p1 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "Gracz1" });
        var p2 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "Gracz2" });

        var league = await _leagueService.CreateLeagueAsync(new CreateLeagueDto
        {
            Name = "Pojedynek Dwóch",
            GameType = BilliardGameType.NineBall,
            RaceTo = 7,
            ParticipantPlayerIds = new List<int> { p1.Id, p2.Id }
        });

        var firstMatch = league.Rounds.First().Matches.First();

        // Act - Gracz 1 wygrywa 7 : 3
        var updatedLeague = await _leagueService.RecordLeagueMatchScoreAsync(firstMatch.LeagueMatchId, 7, 3);

        // Assert
        var leader = updatedLeague.Standings.First();
        leader.PlayerId.Should().Be(firstMatch.Player1Id);
        leader.Wins.Should().Be(1);
        leader.Points.Should().Be(3);
        leader.FramesWon.Should().Be(7);
        leader.FramesLost.Should().Be(3);
        leader.FrameDifference.Should().Be(4);

        var chaser = updatedLeague.Standings.Last();
        chaser.PlayerId.Should().Be(firstMatch.Player2Id);
        chaser.Wins.Should().Be(0);
        chaser.Losses.Should().Be(1);
        chaser.Points.Should().Be(0);
        chaser.FramesWon.Should().Be(3);
        chaser.FramesLost.Should().Be(7);
        chaser.FrameDifference.Should().Be(-4);
    }
}
