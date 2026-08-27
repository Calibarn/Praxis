using Microsoft.EntityFrameworkCore;
using Praxis.Backend.Application.DB;
using Praxis.Backend.Application.Helper;
using Praxis.Backend.Application.Repositories;
using Praxis.Backend.Application.Repositories.Interfaces;
using Praxis.Backend.Application.Service.Seed;

namespace Praxis.Backend.Tests.Integration;

public class SchemaAndSeedTests
{
    [Fact]
    public async Task Migration_schema_and_seed_idempotency()
    {
        var databaseUrl = Environment.GetEnvironmentVariable("NEWS_DATABASE_URL");
        if (string.IsNullOrEmpty(databaseUrl))
        {
            Assert.Skip("NEWS_DATABASE_URL is not configured");
            return;
        }

        await using var context = new NewsDbContext(NewsDbContextOptionsFactory.Build(databaseUrl));
        var repository = new NewsRepository(context);

        try
        {
            await context.Database.MigrateAsync();
            await context.News.ExecuteDeleteAsync();

            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            Assert.Contains(appliedMigrations, m => m.Contains("InitialCreate", StringComparison.Ordinal));

            var timeZone = await context.Database
                .SqlQueryRaw<string>("SELECT @@session.time_zone AS `Value`")
                .SingleAsync();
            Assert.Equal("+00:00", timeZone);

            var indexNames = await context.Database
                .SqlQueryRaw<string>(
                    "SELECT DISTINCT INDEX_NAME AS `Value` FROM INFORMATION_SCHEMA.STATISTICS " +
                    "WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'news' AND INDEX_NAME <> 'PRIMARY'")
                .ToListAsync();
            Assert.Equal(["ix_news_active_published_id"], indexNames);

            var firstCount = await NewsSeeder.SeedNewsAsync(repository, "development", "development");
            var secondCount = await NewsSeeder.SeedNewsAsync(repository, "development", "development");
            var persistedCount = await context.News.CountAsync();

            Assert.Equal(SeedFixtures.DevelopmentSeeds.Count, firstCount);
            Assert.Equal(SeedFixtures.DevelopmentSeeds.Count, secondCount);
            Assert.Equal(SeedFixtures.DevelopmentSeeds.Count, persistedCount);
        }
        finally
        {
            await context.News.ExecuteDeleteAsync();
        }
    }
}
