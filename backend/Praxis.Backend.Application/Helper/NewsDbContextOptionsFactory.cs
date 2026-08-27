using Microsoft.EntityFrameworkCore;
using Praxis.Backend.Application.DB;

namespace Praxis.Backend.Application.Helper;

/// <summary>Builds <see cref="NewsDbContext"/> options consistently for every entry point (host, seed CLI, tests).</summary>
public static class NewsDbContextOptionsFactory
{
    private static readonly MariaDbServerVersion ServerVersion = new(new Version(11, 8, 3));

    public static void Configure(DbContextOptionsBuilder builder, string databaseUrl)
    {
        var connectionString = NewsDatabaseUrl.ToConnectionString(databaseUrl);
        builder
            .UseMySql(connectionString, ServerVersion)
            .AddInterceptors(new UtcSessionTimeZoneInterceptor());
    }

    public static DbContextOptions<NewsDbContext> Build(string databaseUrl)
    {
        var builder = new DbContextOptionsBuilder<NewsDbContext>();
        Configure(builder, databaseUrl);
        return builder.Options;
    }
}
