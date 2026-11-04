using FluentAssertions;
using PoolNet.Application.Services;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;
using Xunit;

namespace PoolNet.UnitTests.Application;

public class LeagueStandingsCalculatorTests
{
    private readonly LeagueStandingsCalculator _calculator = new();

    [Fact]
    public void CalculateStandings_WithCompletedMatches_ComputesCorrectPointsAndPositions()
    {
        // Arrange
        var p1 = new Player("Adam") { Id = 1 };
        var p2 = new Player("Bartek") { Id = 2 };
        var p3 = new Player("Cezary") { Id = 3 };

        var participants = new List<LeagueParticipant>
        {
            new() { PlayerId = 1, Player = p1 },
            new() { PlayerId = 2, Player = p2 },
            new() { PlayerId = 3, Player = p3 }
        };

        // Adam defeats Bartek 5:2
        var m1 = new Match(BilliardGameType.EightBall, MatchKind.League, 1, 2, 5);
        m1.RecordResult(5, 2);

        // Adam defeats Cezary 5:3
        var m2 = new Match(BilliardGameType.EightBall, MatchKind.League, 1, 3, 5);
        m2.RecordResult(5, 3);

        // Bartek defeats Cezary 5:4
        var m3 = new Match(BilliardGameType.EightBall, MatchKind.League, 2, 3, 5);
        m3.RecordResult(5, 4);

        // Scheduled match (not yet played)
        var m4 = new Match(BilliardGameType.EightBall, MatchKind.League, 2, 1, 5);

        var matches = new List<Match> { m1, m2, m3, m4 };

        // Act
        var standings = _calculator.CalculateStandings(participants, matches);

        // Assert
        standings.Should().HaveCount(3);

        // 1. Adam: 2 matches, 2 wins, 6 pts, frames 10:5 (+5)
        var pos1 = standings[0];
        pos1.Position.Should().Be(1);
        pos1.PlayerId.Should().Be(1);
        pos1.Nickname.Should().Be("Adam");
        pos1.MatchesPlayed.Should().Be(2);
        pos1.Wins.Should().Be(2);
        pos1.Losses.Should().Be(0);
        pos1.Points.Should().Be(6);
        pos1.FramesWon.Should().Be(10);
        pos1.FramesLost.Should().Be(5);
        pos1.FrameDifference.Should().Be(5);

        // 2. Bartek: 2 matches, 1 win, 1 loss, 3 pts, frames 7:9 (-2)
        var pos2 = standings[1];
        pos2.Position.Should().Be(2);
        pos2.PlayerId.Should().Be(2);
        pos2.Nickname.Should().Be("Bartek");
        pos2.MatchesPlayed.Should().Be(2);
        pos2.Wins.Should().Be(1);
        pos2.Losses.Should().Be(1);
        pos2.Points.Should().Be(3);
        pos2.FramesWon.Should().Be(7);
        pos2.FramesLost.Should().Be(9);
        pos2.FrameDifference.Should().Be(-2);

        // 3. Cezary: 2 matches, 0 wins, 2 losses, 0 pts, frames 7:10 (-3)
        var pos3 = standings[2];
        pos3.Position.Should().Be(3);
        pos3.PlayerId.Should().Be(3);
        pos3.Nickname.Should().Be("Cezary");
        pos3.MatchesPlayed.Should().Be(2);
        pos3.Wins.Should().Be(0);
        pos3.Losses.Should().Be(2);
        pos3.Points.Should().Be(0);
        pos3.FramesWon.Should().Be(7);
        pos3.FramesLost.Should().Be(10);
        pos3.FrameDifference.Should().Be(-3);
    }

    [Fact]
    public void CalculateStandings_TieBreaking_SortsByFrameDiffThenFramesWonThenNickname()
    {
        // Arrange: two players have 3 pts each, but different frame differences
        var p1 = new Player("GraczB") { Id = 1 };
        var p2 = new Player("GraczA") { Id = 2 };

        var participants = new List<LeagueParticipant>
        {
            new() { PlayerId = 1, Player = p1 },
            new() { PlayerId = 2, Player = p2 }
        };

        // Player 1 wins match 1: 5:4 (+1)
        var m1 = new Match(BilliardGameType.NineBall, MatchKind.League, 1, 2, 5);
        m1.RecordResult(5, 4);

        // Player 2 wins match 2 (return leg): 5:0 (+5)
        var m2 = new Match(BilliardGameType.NineBall, MatchKind.League, 2, 1, 5);
        m2.RecordResult(5, 0);

        var matches = new List<Match> { m1, m2 };

        // Act
        var standings = _calculator.CalculateStandings(participants, matches);

        // Assert
        // GraczA (id 2) has frame difference 9:5 (+4), GraczB (id 1) has 5:9 (-4)
        standings[0].PlayerId.Should().Be(2);
        standings[0].Position.Should().Be(1);
        standings[0].Points.Should().Be(3);
        standings[0].FrameDifference.Should().Be(4);

        standings[1].PlayerId.Should().Be(1);
        standings[1].Position.Should().Be(2);
        standings[1].Points.Should().Be(3);
        standings[1].FrameDifference.Should().Be(-4);
    }

    [Fact]
    public void CalculateStandings_WhenNoCompletedMatches_ReturnsAllParticipantsWithZeroPoints()
    {
        // Arrange
        var p1 = new Player("Jan") { Id = 10 };
        var p2 = new Player("Krzysztof") { Id = 20 };

        var participants = new List<LeagueParticipant>
        {
            new() { PlayerId = 10, Player = p1 },
            new() { PlayerId = 20, Player = p2 }
        };

        var matches = new List<Match>
        {
            new(BilliardGameType.EightBall, MatchKind.League, 10, 20, 5) // Scheduled
        };

        // Act
        var standings = _calculator.CalculateStandings(participants, matches);

        // Assert
        standings.Should().HaveCount(2);
        standings.Should().AllSatisfy(s =>
        {
            s.MatchesPlayed.Should().Be(0);
            s.Points.Should().Be(0);
            s.Wins.Should().Be(0);
            s.Losses.Should().Be(0);
            s.FramesWon.Should().Be(0);
            s.FramesLost.Should().Be(0);
        });
        standings[0].Nickname.Should().Be("Jan");
        standings[1].Nickname.Should().Be("Krzysztof");
    }
}

