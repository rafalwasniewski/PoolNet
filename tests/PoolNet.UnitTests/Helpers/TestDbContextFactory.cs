using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PoolNet.Infrastructure.Data;

namespace PoolNet.UnitTests.Helpers;

public static class TestDbContextFactory
{
    public static (AppDbContext Context, SqliteConnection Connection) CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}
