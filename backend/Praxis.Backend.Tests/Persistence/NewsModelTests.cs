using Microsoft.EntityFrameworkCore;
using Praxis.Backend.Application.DB;
using Praxis.Backend.Application.Helper;

namespace Praxis.Backend.Tests.Persistence;

public class NewsModelTests
{
    private static NewsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NewsDbContext>()
            .UseMySql("Server=unused;Database=unused;User=unused;Password=unused;", new MariaDbServerVersion(new Version(11, 8, 3)))
            .Options;
        return new NewsDbContext(options);
    }

    [Fact]
    public void News_model_owns_the_required_persistence_contract()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(News))!;

        var columnNames = entityType.GetProperties().Select(p => p.GetColumnName()).ToHashSet();
        Assert.Equal(
            new HashSet<string>
            {
                "id", "title", "summary", "content", "published_at", "valid_from",
                "valid_until", "is_active", "created_at", "updated_at",
            },
            columnNames);

        Assert.True(entityType.FindPrimaryKey()!.Properties.Single().Name == nameof(News.Id));
        Assert.True(entityType.FindProperty(nameof(News.ValidUntil))!.IsNullable);
        Assert.False(entityType.FindProperty(nameof(News.IsActive))!.IsNullable);

        var indexNames = entityType.GetIndexes().Select(i => i.GetDatabaseName()).ToHashSet();
        Assert.Equal(new HashSet<string?> { "ix_news_active_published_id" }, indexNames);
    }

    [Fact]
    public void Utc_conversion_rejects_naive_values()
    {
        var converter = UtcDateTimeConversion.Required;
        var naive = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Unspecified);

        var ex = Assert.Throws<ArgumentException>(() => converter.ConvertToProvider(naive));
        Assert.Contains("UTC", ex.Message);
    }

    [Fact]
    public void Utc_conversion_normalizes_values_and_results_to_utc()
    {
        var converter = UtcDateTimeConversion.Required;
        var value = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc);

        var stored = (DateTime)converter.ConvertToProvider(value)!;
        Assert.Equal(DateTimeKind.Unspecified, stored.Kind);
        Assert.Equal(value.Ticks, stored.Ticks);

        var restored = (DateTime)converter.ConvertFromProvider(stored)!;
        Assert.Equal(DateTimeKind.Utc, restored.Kind);
        Assert.Equal(value, restored);
    }

    [Fact]
    public void News_uses_guid_identifiers()
    {
        var news = new News
        {
            Id = Guid.Parse("34b0c18b-95f2-4ea9-ac1e-e7bd4dbce252"),
            Title = "Titel",
            Summary = "Zusammenfassung",
            Content = "Inhalt",
            PublishedAt = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc),
            ValidFrom = new DateTime(2026, 8, 21, 12, 0, 0, DateTimeKind.Utc),
            ValidUntil = null,
            IsActive = true,
        };

        Assert.IsType<Guid>(news.Id);
    }
}
