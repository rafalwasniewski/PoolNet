using FluentAssertions;
using PoolNet.Domain.Entities;
using PoolNet.Domain.Exceptions;
using Xunit;

namespace PoolNet.UnitTests.Domain;

public class PlayerTests
{
    [Fact]
    public void Constructor_WithValidNickname_CreatesPlayer()
    {
        // Act
        var player = new Player("O'Sullivan");

        // Assert
        player.Nickname.Should().Be("O'Sullivan");
        player.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetNickname_WithEmptyOrWhitespace_ThrowsDomainException(string? invalidNickname)
    {
        // Arrange
        var player = new Player();

        // Act
        var act = () => player.SetNickname(invalidNickname!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*nie może być pusty*");
    }

    [Fact]
    public void SetNickname_TrimsWhitespace()
    {
        // Arrange
        var player = new Player();

        // Act
        player.SetNickname("   Efren Reyes   ");

        // Assert
        player.Nickname.Should().Be("Efren Reyes");
    }
}
