using Microsoft.EntityFrameworkCore;
using Praxis.Backend.Application.Helper;

namespace Praxis.Backend.Application.DB;

public sealed class NewsDbContext(DbContextOptions<NewsDbContext> options) : DbContext(options)
{
    public DbSet<News> News => Set<News>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<News>(entity =>
        {
            entity.ToTable("news");
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id)
                .HasColumnName("id")
                .HasConversion<string>()
                .HasMaxLength(36);
            entity.Property(n => n.Title).HasColumnName("title").HasMaxLength(240).IsRequired();
            entity.Property(n => n.Summary).HasColumnName("summary").HasColumnType("text").IsRequired();
            entity.Property(n => n.Content).HasColumnName("content").HasColumnType("text").IsRequired();
            entity.Property(n => n.PublishedAt)
                .HasColumnName("published_at")
                .HasConversion(UtcDateTimeConversion.Required)
                .IsRequired();
            entity.Property(n => n.ValidFrom)
                .HasColumnName("valid_from")
                .HasConversion(UtcDateTimeConversion.Required)
                .IsRequired();
            entity.Property(n => n.ValidUntil)
                .HasColumnName("valid_until")
                .HasConversion(UtcDateTimeConversion.Nullable);
            entity.Property(n => n.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();
            entity.Property(n => n.CreatedAt)
                .HasColumnName("created_at")
                .HasConversion(UtcDateTimeConversion.Computed)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(n => n.UpdatedAt)
                .HasColumnName("updated_at")
                .HasConversion(UtcDateTimeConversion.Computed)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .ValueGeneratedOnAddOrUpdate()
                .IsRequired();

            entity.HasIndex(n => new { n.IsActive, n.PublishedAt, n.Id })
                .HasDatabaseName("ix_news_active_published_id");
        });
    }
}
