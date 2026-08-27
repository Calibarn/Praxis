using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Praxis.Backend.Application.DB;
using Praxis.Backend.Application.Helper;
using Praxis.Backend.Application.Repositories;
using Praxis.Backend.Application.Repositories.Interfaces;

namespace Praxis.Backend.Tests.Integration;

public class FullStackTests
{
    [Fact]
    public async Task Full_stack_lists_a_seeded_news_item_through_the_real_database()
    {
        var databaseUrl = Environment.GetEnvironmentVariable("NEWS_DATABASE_URL");
        if (string.IsNullOrEmpty(databaseUrl))
        {
            Assert.Skip("NEWS_DATABASE_URL is not configured");
            return;
        }

        await using var context = new NewsDbContext(NewsDbContextOptionsFactory.Build(databaseUrl));
        var repository = new NewsRepository(context);
        var seedId = Guid.Parse("00000000-0000-0000-0000-0000000000ff");

        try
        {
            await context.Database.MigrateAsync();
            await context.News.ExecuteDeleteAsync();
            await repository.UpsertAsync(new News
            {
                Id = seedId,
                Title = "Titel",
                Summary = "Zusammenfassung",
                Content = "Inhalt",
                PublishedAt = DateTime.UtcNow.AddHours(-1),
                ValidFrom = DateTime.UtcNow.AddHours(-1),
                ValidUntil = null,
                IsActive = true,
            });
            await repository.SaveChangesAsync();

            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.UseSetting("NEWS_DATABASE_URL", databaseUrl));
            using var client = factory.CreateClient();

            var listResponse = await client.GetAsync("/api/news");
            var healthResponse = await client.GetAsync("/health");

            Assert.Equal(System.Net.HttpStatusCode.OK, listResponse.StatusCode);
            var body = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(1, body.GetProperty("total").GetInt32());
            Assert.Equal(seedId.ToString(), body.GetProperty("items")[0].GetProperty("id").GetString());
            Assert.Equal(System.Net.HttpStatusCode.OK, healthResponse.StatusCode);
        }
        finally
        {
            await context.News.ExecuteDeleteAsync();
        }
    }
}
