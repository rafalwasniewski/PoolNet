using FluentAssertions;
using PoolNet.Application.Services;
using PoolNet.Domain.Exceptions;
using Xunit;

namespace PoolNet.UnitTests.Application;

public class TournamentBracketGeneratorTests
{
    private readonly TournamentBracketGenerator _generator = new();

    [Fact]
    public void GenerateBracket_For4Players_Creates3MatchesAcross2Rounds()
    {
        // Arrange
        var players = new List<int> { 1, 2, 3, 4 };

        // Act
        var bracket = _generator.GenerateBracket(players);

        // Assert
        bracket.Should().HaveCount(3); // 2 w półfinale + 1 finał

        var round1 = bracket.Where(b => b.RoundNumber == 1).ToList();
        var round2 = bracket.Where(b => b.RoundNumber == 2).ToList();

        round1.Should().HaveCount(2);
        round2.Should().HaveCount(1); // Finał

        // Obaj zwycięzcy z rundy 1 powinni celować w finał
        var finalMatch = round2.Single();
        round1.All(m => m.NextTempId == finalMatch.TempId).Should().BeTrue();
        round1[0].NextMatchSlot.Should().Be(1);
        round1[1].NextMatchSlot.Should().Be(2);
    }

    [Fact]
    public void GenerateBracket_For8Players_Creates7MatchesAcross3Rounds()
    {
        // Arrange
        var players = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };

        // Act
        var bracket = _generator.GenerateBracket(players);

        // Assert
        bracket.Should().HaveCount(7); // 4 + 2 + 1
        bracket.Count(b => b.RoundNumber == 1).Should().Be(4);
        bracket.Count(b => b.RoundNumber == 2).Should().Be(2);
        bracket.Count(b => b.RoundNumber == 3).Should().Be(1);
    }

    [Fact]
    public void GenerateBracket_For3Players_HandlesByeCorrectly()
    {
        // Arrange
        var players = new List<int> { 1, 2, 3 };

        // Act
        var bracket = _generator.GenerateBracket(players);

        // Assert
        bracket.Should().HaveCount(3); // Rozmiar 4-osobowy

        var byeMatch = bracket.Single(b => b.RoundNumber == 1 && b.IsBye);
        byeMatch.Player1Id.Should().NotBeNull();
        byeMatch.Player2Id.Should().BeNull();

        // Gracz z wolnym losem powinien zostać automatycznie przypisany do finału (runda 2)
        var finalMatch = bracket.Single(b => b.RoundNumber == 2);
        (finalMatch.Player1Id == byeMatch.Player1Id || finalMatch.Player2Id == byeMatch.Player1Id)
            .Should().BeTrue("Gracz z wolnym losem awansuje automatycznie do finału");
    }

    [Fact]
    public void GenerateBracket_WhenLessThan2Players_ThrowsDomainException()
    {
        // Act
        var act = () => _generator.GenerateBracket(new List<int> { 10 });

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*co najmniej 2*");
    }
}
