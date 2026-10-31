using FluentAssertions;
using PoolNet.Application.DTOs;
using PoolNet.Application.Services;
using PoolNet.Domain.Enums;
using PoolNet.UnitTests.Helpers;
using Xunit;

namespace PoolNet.UnitTests.Services;

public class TournamentServiceTests : IDisposable
{
    private readonly PoolNet.Infrastructure.Data.AppDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly TournamentService _tournamentService;
    private readonly PlayerService _playerService;

    public TournamentServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.CreateInMemoryDbContext();
        var bracketGen = new TournamentBracketGenerator();
        _tournamentService = new TournamentService(_context, bracketGen);
        _playerService = new PlayerService(_context, new PlayerStatisticsCalculator());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateTournamentAsync_For4Players_CreatesBracketWithSemifinalsAndFinal()
    {
        // Arrange
        var p1 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "P1" });
        var p2 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "P2" });
        var p3 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "P3" });
        var p4 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "P4" });

        var dto = new CreateTournamentDto
        {
            Name = "Grand Prix 10-Ball",
            GameType = BilliardGameType.TenBall,
            RaceTo = 5,
            ParticipantPlayerIds = new List<int> { p1.Id, p2.Id, p3.Id, p4.Id }
        };

        // Act
        var tournament = await _tournamentService.CreateTournamentAsync(dto);

        // Assert
        tournament.Should().NotBeNull();
        tournament.Rounds.Should().HaveCount(2); // Półfinały + Finał
        tournament.Status.Should().Be(TournamentStatus.InProgress);

        var semis = tournament.Rounds.First(r => r.RoundNumber == 1);
        semis.Matches.Should().HaveCount(2);

        var final = tournament.Rounds.First(r => r.RoundNumber == 2);
        final.Matches.Should().HaveCount(1);
    }

    [Fact]
    public async Task RecordTournamentMatchScoreAsync_AdvancesWinnerToFinal_AndFinishesTournament()
    {
        // Arrange
        var p1 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "P1" });
        var p2 = await _playerService.CreatePlayerAsync(new CreatePlayerDto { Nickname = "P2" });

        var tournament = await _tournamentService.CreateTournamentAsync(new CreateTournamentDto
        {
            Name = "Super Finał",
            GameType = BilliardGameType.EightBall,
            RaceTo = 5,
            ParticipantPlayerIds = new List<int> { p1.Id, p2.Id }
        });

        // W turnieju 2-osobowym jest tylko 1 runda (Finał)
        var finalMatch = tournament.Rounds.First().Matches.First();

        // Act - P1 wygrywa 5:2
        var finished = await _tournamentService.RecordTournamentMatchScoreAsync(finalMatch.TournamentMatchId, 5, 2);

        // Assert
        finished.Status.Should().Be(TournamentStatus.Finished);
        finished.WinnerId.Should().Be(p1.Id);
        finished.WinnerNickname.Should().Be("P1");
    }
}
