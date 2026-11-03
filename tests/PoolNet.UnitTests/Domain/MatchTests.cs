using FluentAssertions;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Enums;
using PoolNet.Domain.Exceptions;
using Xunit;

namespace PoolNet.UnitTests.Domain;

public class MatchTests
{
    [Fact]
    public void Constructor_WithValidParameters_InitializesMatchCorrectly()
    {
        // Act
        var match = new Match(BilliardGameType.EightBall, MatchKind.Friendly, 1, 2, 5);

        // Assert
        match.GameType.Should().Be(BilliardGameType.EightBall);
        match.MatchKind.Should().Be(MatchKind.Friendly);
        match.Player1Id.Should().Be(1);
        match.Player2Id.Should().Be(2);
        match.FirstToWins.Should().Be(5);
        match.Status.Should().Be(MatchStatus.Scheduled);
        match.WinnerId.Should().BeNull();
    }

    [Fact]
    public void Constructor_WhenPlayerPlaysThemselves_ThrowsDomainException()
    {
        // Act
        var act = () => new Match(BilliardGameType.EightBall, MatchKind.Friendly, 1, 1, 5);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*z samym sobą*");
    }

    [Fact]
    public void Constructor_WhenFirstToWinsIsZeroOrNegative_ThrowsDomainException()
    {
        // Act
        var act = () => new Match(BilliardGameType.NineBall, MatchKind.Friendly, 1, 2, 0);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*większa od zera*");
    }

    [Fact]
    public void RecordResult_WhenPlayer1Wins_SetsWinnerAndCompletedStatus()
    {
        // Arrange
        var match = new Match(BilliardGameType.EightBall, MatchKind.Friendly, 10, 20, 5);

        // Act
        match.RecordResult(5, 3);

        // Assert
        match.Status.Should().Be(MatchStatus.Completed);
        match.Player1Score.Should().Be(5);
        match.Player2Score.Should().Be(3);
        match.WinnerId.Should().Be(10);
        match.PlayedAt.Should().NotBeNull();
    }

    [Fact]
    public void RecordResult_WhenPlayer2Wins_SetsWinnerAndCompletedStatus()
    {
        // Arrange
        var match = new Match(BilliardGameType.TenBall, MatchKind.League, 10, 20, 7);

        // Act
        match.RecordResult(4, 7);

        // Assert
        match.Status.Should().Be(MatchStatus.Completed);
        match.Player1Score.Should().Be(4);
        match.Player2Score.Should().Be(7);
        match.WinnerId.Should().Be(20);
    }

    [Fact]
    public void RecordResult_WhenScoreExceedsFirstToWins_ThrowsDomainException()
    {
        // Arrange
        var match = new Match(BilliardGameType.EightBall, MatchKind.Friendly, 1, 2, 5);

        // Act
        var act = () => match.RecordResult(6, 2);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*nie może wygrać więcej partii*");
    }

    [Fact]
    public void RecordResult_WhenBothPlayersHaveMaxScore_ThrowsDomainException()
    {
        // Arrange
        var match = new Match(BilliardGameType.EightBall, MatchKind.Friendly, 1, 2, 5);

        // Act
        var act = () => match.RecordResult(5, 5);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*nie ma remisów*");
    }

    [Fact]
    public void RecordResult_WhenNoPlayerReachedTarget_ThrowsDomainException()
    {
        // Arrange
        var match = new Match(BilliardGameType.EightBall, MatchKind.Friendly, 1, 2, 5);

        // Act
        var act = () => match.RecordResult(4, 3);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*dokładnie 5 wygranych partii*");
    }

    [Fact]
    public void RecordResult_WhenScoreIsNegative_ThrowsDomainException()
    {
        // Arrange
        var match = new Match(BilliardGameType.EightBall, MatchKind.Friendly, 1, 2, 5);

        // Act
        var act = () => match.RecordResult(-1, 5);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*nie może być ujemny*");
    }

    [Fact]
    public void RecordResult_WhenResultIsCorrected_PreservesOriginalPlayedAt()
    {
        // Arrange
        var match = new Match(BilliardGameType.EightBall, MatchKind.League, 1, 2, 5);
        var playedAt = new DateTime(2026, 1, 10, 18, 30, 0, DateTimeKind.Utc);
        match.RecordResult(5, 2, playedAt);

        // Act
        match.RecordResult(2, 5);

        // Assert
        match.WinnerId.Should().Be(2);
        match.PlayedAt.Should().Be(playedAt);
    }
}
