using FluentAssertions;
using PoolNet.Application.Services;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;
using Xunit;

namespace PoolNet.UnitTests.Application;

public class PlayerStatisticsCalculatorTests
{
    private readonly PlayerStatisticsCalculator _calculator = new();

    [Fact]
    public void CalculateOverall_CalculatesAccurateTotalsAndIgnoresScheduledMatches()
    {
        // Arrange
        int playerId = 1;
        int opponentId = 2;

        var m1 = new Match(BilliardGameType.EightBall, MatchKind.Friendly, playerId, opponentId, 5);
        m1.RecordResult(5, 2); // Win for Player 1

        var m2 = new Match(BilliardGameType.NineBall, MatchKind.League, opponentId, playerId, 7);
        m2.RecordResult(7, 4); // Loss for Player 1 (4 frames won, 7 lost)

        var m3 = new Match(BilliardGameType.TenBall, MatchKind.Tournament, playerId, opponentId, 5);
        // Scheduled, not completed yet

        var matches = new List<Match> { m1, m2, m3 };

        // Act
        var stats = _calculator.CalculateOverall(playerId, matches);

        // Assert
        stats.TotalMatches.Should().Be(2);
        stats.Wins.Should().Be(1);
        stats.Losses.Should().Be(1);
        stats.WinRate.Should().Be(50.0);
        stats.FramesWon.Should().Be(5 + 4); // 9
        stats.FramesLost.Should().Be(2 + 7); // 9
        stats.FrameDifference.Should().Be(0);
    }

    [Fact]
    public void CalculatePerGameType_CalculatesStatsGroupedByDiscipline()
    {
        // Arrange
        int playerId = 1;
        int opp = 2;

        var m1 = new Match(BilliardGameType.EightBall, MatchKind.Friendly, playerId, opp, 5);
        m1.RecordResult(5, 1);

        var m2 = new Match(BilliardGameType.EightBall, MatchKind.Friendly, opp, playerId, 5);
        m2.RecordResult(3, 5); // Win for playerId as Player2

        var m3 = new Match(BilliardGameType.NineBall, MatchKind.Friendly, playerId, opp, 7);
        m3.RecordResult(3, 7); // Loss for playerId

        var matches = new List<Match> { m1, m2, m3 };

        // Act
        var perType = _calculator.CalculatePerGameType(playerId, matches);

        // Assert
        perType.Should().HaveCount(3);

        var eightBall = perType.Single(s => s.GameType == BilliardGameType.EightBall);
        eightBall.TotalMatches.Should().Be(2);
        eightBall.Wins.Should().Be(2);
        eightBall.Losses.Should().Be(0);
        eightBall.WinRate.Should().Be(100.0);
        eightBall.FramesWon.Should().Be(5 + 5);
        eightBall.FramesLost.Should().Be(1 + 3);

        var nineBall = perType.Single(s => s.GameType == BilliardGameType.NineBall);
        nineBall.TotalMatches.Should().Be(1);
        nineBall.Wins.Should().Be(0);
        nineBall.Losses.Should().Be(1);
        nineBall.WinRate.Should().Be(0.0);
        nineBall.FramesWon.Should().Be(3);
        nineBall.FramesLost.Should().Be(7);

        var tenBall = perType.Single(s => s.GameType == BilliardGameType.TenBall);
        tenBall.TotalMatches.Should().Be(0);
        tenBall.Wins.Should().Be(0);
    }
}
