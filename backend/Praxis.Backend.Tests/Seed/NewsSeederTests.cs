using Praxis.Backend.Application.Service.Seed;

namespace Praxis.Backend.Tests.Seed;

public class NewsSeederTests
{
    [Fact]
    public void Development_seeds_have_stable_unique_ids()
    {
        var ids = SeedFixtures.DevelopmentSeeds.Select(s => s.Id).ToList();

        Assert.NotEmpty(ids);
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Development_seeds_are_explicitly_non_production()
    {
        var allSeeds = SeedFixtures.DevelopmentSeeds.Concat(SeedFixtures.TestSeeds);
        Assert.All(allSeeds, seed => Assert.Contains(seed.Environment, new[] { "development", "test" }));
    }

    [Fact]
    public void Seed_rejects_every_unsupported_environment()
    {
        var ex = Assert.Throws<ArgumentException>(() => NewsSeeder.ValidateEnvironments("production", "test"));
        Assert.Contains("development or test", ex.Message);
    }

    [Fact]
    public void Seed_rejects_production_target_before_database_access()
    {
        var ex = Assert.Throws<ArgumentException>(() => NewsSeeder.ValidateEnvironments("development", "production"));
        Assert.Contains("production target", ex.Message);
    }

    [Fact]
    public void Seed_environment_must_match_target_environment()
    {
        var ex = Assert.Throws<ArgumentException>(() => NewsSeeder.ValidateEnvironments("development", "test"));
        Assert.Contains("must match", ex.Message);
    }

    [Theory]
    [InlineData("development", "production")]
    [InlineData("development", "test")]
    [InlineData("staging", "staging")]
    public void ValidateEnvironments_rejects_unsafe_targets_without_a_repository(
        string environment, string targetEnvironment)
    {
        Assert.Throws<ArgumentException>(() => NewsSeeder.ValidateEnvironments(environment, targetEnvironment));
    }
}
