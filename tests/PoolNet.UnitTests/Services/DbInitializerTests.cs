using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PoolNet.Domain.Entities;
using PoolNet.Infrastructure.Data;
using PoolNet.UnitTests.Helpers;
using Xunit;

namespace PoolNet.UnitTests.Services;

public class DbInitializerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

    public DbInitializerTests()
    {
        (_context, _connection) = TestDbContextFactory.CreateInMemoryDbContext();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task InitializeAsync_WithExistingData_DoesNotDeleteOrReseed()
    {
        // Arrange
        _context.Players.Add(new Player("Existing Player"));
        await _context.SaveChangesAsync();

        // Act
        await DbInitializer.InitializeAsync(_context);

        // Assert
        var players = await _context.Players.ToListAsync();
        players.Should().ContainSingle();
        players[0].Nickname.Should().Be("Existing Player");
    }
}
