using Microsoft.EntityFrameworkCore;
using PoolNet.Domain.Entities;

namespace PoolNet.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Player> Players { get; }
    DbSet<Match> Matches { get; }
    DbSet<Tournament> Tournaments { get; }
    DbSet<TournamentParticipant> TournamentParticipants { get; }
    DbSet<TournamentMatch> TournamentMatches { get; }
    DbSet<League> Leagues { get; }
    DbSet<LeagueParticipant> LeagueParticipants { get; }
    DbSet<LeagueMatch> LeagueMatches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
