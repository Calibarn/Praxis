using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Praxis.Backend.Application.DB;

namespace Praxis.Backend.Tests.Api;

public class NewsApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static News NewsItem(string id) => new()
    {
        Id = Guid.Parse(id),
        Title = "Titel",
        Summary = "Zusammenfassung",
        Content = "Inhalt",
        PublishedAt = new DateTime(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc),
        ValidFrom = new DateTime(2026, 8, 26, 8, 0, 0, DateTimeKind.Utc),
        ValidUntil = null,
        IsActive = true,
    };

    [Fact]
    public async Task List_news_returns_camel_case_page_matching_the_contract()
    {
        var item = NewsItem("00000000-0000-0000-0000-000000000001");
        using var factory = new NewsApiTestFactory(() => new FakeNewsRepository([item], total: 1));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/news?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("page").GetInt32());
        Assert.Equal(20, body.GetProperty("pageSize").GetInt32());
        Assert.Equal(1, body.GetProperty("total").GetInt32());
        Assert.False(body.GetProperty("hasMore").GetBoolean());

        var first = body.GetProperty("items")[0];
        Assert.Equal("00000000-0000-0000-0000-000000000001", first.GetProperty("id").GetString());
        Assert.Equal("Titel", first.GetProperty("title").GetString());
        Assert.Equal("Zusammenfassung", first.GetProperty("summary").GetString());
        Assert.Equal("Inhalt", first.GetProperty("content").GetString());
        Assert.Equal("2026-08-26T08:00:00Z", first.GetProperty("publishedAt").GetString());
        Assert.Equal("2026-08-26T08:00:00Z", first.GetProperty("validFrom").GetString());
        Assert.Equal(JsonValueKind.Null, first.GetProperty("validUntil").ValueKind);
    }

    [Fact]
    public async Task List_news_defaults_to_page_1_and_page_size_20()
    {
        using var factory = new NewsApiTestFactory(() => new FakeNewsRepository([], total: 0));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/news");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, body.GetProperty("page").GetInt32());
        Assert.Equal(20, body.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task List_news_reports_has_more_when_further_pages_remain()
    {
        var item = NewsItem("00000000-0000-0000-0000-000000000002");
        using var factory = new NewsApiTestFactory(() => new FakeNewsRepository([item], total: 5));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/news?page=1&pageSize=1");

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("hasMore").GetBoolean());
    }

    [Fact]
    public async Task List_news_reports_no_more_pages_on_the_last_page()
    {
        var item = NewsItem("00000000-0000-0000-0000-000000000003");
        using var factory = new NewsApiTestFactory(() => new FakeNewsRepository([item], total: 1));
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/news?page=1&pageSize=20");

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("hasMore").GetBoolean());
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("page=-1")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    public async Task List_news_rejects_out_of_range_parameters_with_a_stable_problem_shape(string query)
    {
        using var factory = new NewsApiTestFactory(() => new FakeNewsRepository([], total: 0));
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/news?{query}");

        Assert.Equal((HttpStatusCode)422, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, body.EnumerateObject().Count());
        Assert.Equal("invalid_request", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task List_news_maps_database_failures_to_503_without_leaking_details()
    {
        using var factory = new NewsApiTestFactory(() => new FailingNewsRepository());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/news");

        Assert.Equal((HttpStatusCode)503, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, body.EnumerateObject().Count());
        Assert.DoesNotContain("connection refused", body.GetProperty("message").GetString());
        Assert.Equal("news_service_unavailable", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Health_reports_ok_when_the_database_is_reachable()
    {
        using var factory = new NewsApiTestFactory(() => new FakeNewsRepository([], total: 0) { IsHealthy = true });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("ok", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Health_reports_503_when_the_database_is_unreachable()
    {
        using var factory = new NewsApiTestFactory(() => new FakeNewsRepository([], total: 0) { IsHealthy = false });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal((HttpStatusCode)503, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.DoesNotContain("connection refused", body.GetProperty("message").GetString());
    }
}
