using Praxis.Backend.Application.Repositories.Interfaces;

namespace Praxis.Backend.Application.Service.Seed;

public static class NewsSeeder
{
    private static readonly HashSet<string> AllowedEnvironments = ["development", "test"];

    /// <summary>
    /// Validates a requested seed environment/target combination without
    /// touching any repository or database connection. Callers should run
    /// this before creating a database engine for an untrusted target.
    /// </summary>
    public static void ValidateEnvironments(string environment, string targetEnvironment)
    {
        if (targetEnvironment == "production")
        {
            throw new ArgumentException("Seeds are forbidden for a production target");
        }

        if (!AllowedEnvironments.Contains(targetEnvironment))
        {
            throw new ArgumentException("Seed target environment must be development or test");
        }

        if (!AllowedEnvironments.Contains(environment))
        {
            throw new ArgumentException("Seed environment must be development or test");
        }

        if (environment != targetEnvironment)
        {
            throw new ArgumentException("Seed environment must match target environment");
        }
    }

    /// <summary>Seeds non-production News data. Validates before touching <paramref name="repository"/>.</summary>
    public static async Task<int> SeedNewsAsync(
        INewsRepository repository,
        string environment,
        string targetEnvironment,
        CancellationToken cancellationToken = default)
    {
        ValidateEnvironments(environment, targetEnvironment);

        var seeds = environment == "development" ? SeedFixtures.DevelopmentSeeds : SeedFixtures.TestSeeds;

        await repository.UpsertManyAsync(seeds.Select(seed => seed.ToModel()), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.CountByIdsAsync(seeds.Select(seed => seed.Id), cancellationToken);
    }
}
