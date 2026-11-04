using Microsoft.Extensions.DependencyInjection;
using PoolNet.Application.Interfaces;
using PoolNet.Application.Services;

namespace PoolNet.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPlayerStatisticsCalculator, PlayerStatisticsCalculator>();
        services.AddScoped<ILeagueScheduleGenerator, LeagueScheduleGenerator>();
        services.AddScoped<ILeagueStandingsCalculator, LeagueStandingsCalculator>();
        services.AddScoped<ITournamentBracketGenerator, TournamentBracketGenerator>();

        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<ILeagueService, LeagueService>();
        services.AddScoped<ITournamentService, TournamentService>();

        return services;
    }
}
