using Microsoft.EntityFrameworkCore;
using PoolNet.Application.Interfaces;
using PoolNet.Domain.Entities;

namespace PoolNet.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentParticipant> TournamentParticipants => Set<TournamentParticipant>();
    public DbSet<TournamentMatch> TournamentMatches => Set<TournamentMatch>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueParticipant> LeagueParticipants => Set<LeagueParticipant>();
    public DbSet<LeagueMatch> LeagueMatches => Set<LeagueMatch>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Player
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nickname)
                .IsRequired()
                .HasMaxLength(50)
                .UseCollation("NOCASE");
            entity.HasIndex(e => e.Nickname)
                .IsUnique();
        });

        // Match
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Player1)
                .WithMany(p => p.MatchesAsPlayer1)
                .HasForeignKey(e => e.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Player2)
                .WithMany(p => p.MatchesAsPlayer2)
                .HasForeignKey(e => e.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Winner)
                .WithMany()
                .HasForeignKey(e => e.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Tournament
        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Winner)
                .WithMany()
                .HasForeignKey(e => e.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Participants)
                .WithOne(p => p.Tournament)
                .HasForeignKey(p => p.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Matches)
                .WithOne(m => m.Tournament)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TournamentParticipant
        modelBuilder.Entity<TournamentParticipant>(entity =>
        {
            entity.HasKey(e => new { e.TournamentId, e.PlayerId });

            entity.HasOne(e => e.Player)
                .WithMany(p => p.TournamentParticipations)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TournamentMatch
        modelBuilder.Entity<TournamentMatch>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.NextTournamentMatch)
                .WithMany()
                .HasForeignKey(e => e.NextTournamentMatchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Match)
                .WithMany()
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Player1)
                .WithMany()
                .HasForeignKey(e => e.Player1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Player2)
                .WithMany()
                .HasForeignKey(e => e.Player2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // League
        modelBuilder.Entity<League>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasMany(e => e.Participants)
                .WithOne(p => p.League)
                .HasForeignKey(p => p.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Matches)
                .WithOne(m => m.League)
                .HasForeignKey(m => m.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LeagueParticipant
        modelBuilder.Entity<LeagueParticipant>(entity =>
        {
            entity.HasKey(e => new { e.LeagueId, e.PlayerId });

            entity.HasOne(e => e.Player)
                .WithMany(p => p.LeagueParticipations)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LeagueMatch
        modelBuilder.Entity<LeagueMatch>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Match)
                .WithMany()
                .HasForeignKey(e => e.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
