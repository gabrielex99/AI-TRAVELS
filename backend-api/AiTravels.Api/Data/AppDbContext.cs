using AiTravels.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiTravels.Api.Data;

/// <summary>
/// Application database context for PostgreSQL.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TripEntity> Trips { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TripEntity>(entity =>
        {
            entity.ToTable("trips");

            entity.HasKey(e => e.Id);

            // Unique index on search_hash for cache lookups
            entity.HasIndex(e => e.SearchHash)
                .IsUnique()
                .HasDatabaseName("ix_trips_search_hash");

            // Index on slug for URL-based lookups
            entity.HasIndex(e => e.Slug)
                .HasDatabaseName("ix_trips_slug");

            // Index on created_at for sorting
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("ix_trips_created_at");

            // JSONB column configuration
            entity.Property(e => e.ItineraryData)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'{}'::jsonb");

            entity.Property(e => e.AffiliateData)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'{}'::jsonb");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("NOW()");
        });
    }
}
