using FluentAssertions;
using PoolNet.Application.Services;
using PoolNet.Domain.Exceptions;
using Xunit;

namespace PoolNet.UnitTests.Application;

public class LeagueScheduleGeneratorTests
{
    private readonly LeagueScheduleGenerator _generator = new();

    [Fact]
    public void GenerateDoubleRoundRobin_For4Players_Generates12MatchesAcross6Rounds()
    {
        // Arrange
        var players = new List<int> { 1, 2, 3, 4 };

        // Act
        var schedule = _generator.GenerateDoubleRoundRobin(players);

        // Assert
        // N * (N - 1) = 4 * 3 = 12 matches
        schedule.Should().HaveCount(12);

        // 6 rounds (3 regular season rounds, 3 return fixture rounds)
        schedule.Select(x => x.RoundNumber).Distinct().Should().HaveCount(6);

        // Each pair (A, B) must play exactly 2 matches: once A as host, once B as host
        foreach (var p1 in players)
        {
            foreach (var p2 in players)
            {
                if (p1 == p2) continue;

                var directMatch = schedule.SingleOrDefault(m => m.Player1Id == p1 && m.Player2Id == p2);
                directMatch.Should().NotBeNull($"Musi istnieć mecz, gdzie Gracz {p1} podejmuje Gracza {p2}");
            }
        }
    }

    [Fact]
    public void GenerateDoubleRoundRobin_For3Players_Generates6MatchesWithByesHandled()
    {
        // Arrange
        var players = new List<int> { 1, 2, 3 };

        // Act
        var schedule = _generator.GenerateDoubleRoundRobin(players);

        // Assert
        // 3 * 2 = 6 matches
        schedule.Should().HaveCount(6);

        // No match should contain the dummy player (-1)
        schedule.Should().NotContain(m => m.Player1Id == -1 || m.Player2Id == -1);

        foreach (var p1 in players)
        {
            foreach (var p2 in players)
            {
                if (p1 == p2) continue;

                var directMatch = schedule.SingleOrDefault(m => m.Player1Id == p1 && m.Player2Id == p2);
                directMatch.Should().NotBeNull();
            }
        }
    }

    [Fact]
    public void GenerateDoubleRoundRobin_WhenLessThan2Players_ThrowsDomainException()
    {
        // Act
        var act = () => _generator.GenerateDoubleRoundRobin(new List<int> { 1 });

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*co najmniej 2*");
    }

    [Fact]
    public void GenerateDoubleRoundRobin_WhenDuplicatesInPlayers_ThrowsDomainException()
    {
        // Act
        var act = () => _generator.GenerateDoubleRoundRobin(new List<int> { 1, 2, 2, 3 });

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*duplikaty*");
    }
}
