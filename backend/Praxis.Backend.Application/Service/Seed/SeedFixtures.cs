namespace Praxis.Backend.Application.Service.Seed;

public static class SeedFixtures
{
    public static readonly IReadOnlyList<NewsSeed> DevelopmentSeeds =
    [
        new NewsSeed(
            Id: Guid.Parse("18ee8f7d-a5b7-4db2-a0da-40558ae96779"),
            Environment: "development",
            Title: "Willkommen in der Praxis",
            Summary: "Die neue Praxis-Website ist im Aufbau.",
            Content: "Hier finden Sie künftig aktuelle Informationen aus unserer Praxis.",
            PublishedAt: new DateTime(2026, 8, 21, 8, 0, 0, DateTimeKind.Utc),
            ValidFrom: new DateTime(2026, 8, 21, 8, 0, 0, DateTimeKind.Utc)),
        new NewsSeed(
            Id: Guid.Parse("be0d4355-48b3-4010-98f4-a9563512bb22"),
            Environment: "development",
            Title: "Hinweis zu Sprechzeiten",
            Summary: "Bitte vereinbaren Sie vor Ihrem Besuch einen Termin.",
            Content: "Terminvereinbarungen helfen uns, Wartezeiten möglichst kurz zu halten.",
            PublishedAt: new DateTime(2026, 8, 20, 8, 0, 0, DateTimeKind.Utc),
            ValidFrom: new DateTime(2026, 8, 20, 8, 0, 0, DateTimeKind.Utc)),
    ];

    public static readonly IReadOnlyList<NewsSeed> TestSeeds =
    [
        new NewsSeed(
            Id: Guid.Parse("41bc655a-0a65-4cc7-b4be-e8150137e671"),
            Environment: "test",
            Title: "Testnachricht",
            Summary: "Testzusammenfassung",
            Content: "<script>alert('als Text behandeln')</script>",
            PublishedAt: new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc),
            ValidFrom: new DateTime(2026, 8, 21, 10, 0, 0, DateTimeKind.Utc)),
    ];
}
