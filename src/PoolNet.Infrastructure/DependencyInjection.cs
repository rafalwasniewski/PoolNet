using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PoolNet.Application.Interfaces;
using PoolNet.Infrastructure.Data;

namespace PoolNet.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}
