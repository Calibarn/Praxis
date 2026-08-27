using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Praxis.Backend.Application.DB;
using Praxis.Backend.Application.Helper;
using Praxis.Backend.Application.Repositories;
using Praxis.Backend.Application.Repositories.Interfaces;
using Praxis.Backend.Application.Service.Seed;

if (args.Length > 0 && args[0] == "seed")
{
    return await RunSeedAsync(args.Skip(1).ToArray());
}

if (args.Contains("--migrate"))
{
    return await RunMigrateAsync();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var databaseUrl = builder.Configuration["NEWS_DATABASE_URL"]
    ?? throw new InvalidOperationException("NEWS_DATABASE_URL must be set");

builder.Services.AddDbContext<NewsDbContext>(options => NewsDbContextOptionsFactory.Configure(options, databaseUrl));
builder.Services.AddScoped<INewsRepository, NewsRepository>();

var app = builder.Build();

app.MapControllers();
app.Run();
return 0;

static async Task<int> RunMigrateAsync()
{
    // Deliberately separate from NEWS_DATABASE_URL: migrations need DDL rights
    // (CREATE/ALTER/DROP) that the long-running app's runtime DB user must not
    // have, so a compromised app process can't also alter the schema.
    // See docs/architecture/threat-model.md, "DB privilege separation".
    var migrationDatabaseUrl = Environment.GetEnvironmentVariable("NEWS_MIGRATION_DATABASE_URL");
    if (string.IsNullOrEmpty(migrationDatabaseUrl))
    {
        Console.Error.WriteLine("NEWS_MIGRATION_DATABASE_URL must be set to run migrations");
        return 1;
    }

    await using var context = new NewsDbContext(NewsDbContextOptionsFactory.Build(migrationDatabaseUrl));
    await context.Database.MigrateAsync();
    return 0;
}

static async Task<int> RunSeedAsync(string[] seedArgs)
{
    if (seedArgs.Length != 1 || (seedArgs[0] != "development" && seedArgs[0] != "test"))
    {
        Console.Error.WriteLine("usage: <app> seed <development|test>");
        return 1;
    }

    var environment = seedArgs[0];

    var databaseUrl = Environment.GetEnvironmentVariable("NEWS_DATABASE_URL");
    if (string.IsNullOrEmpty(databaseUrl))
    {
        Console.Error.WriteLine("NEWS_DATABASE_URL must be set");
        return 1;
    }

    var targetEnvironment = Environment.GetEnvironmentVariable("NEWS_DEPLOYMENT_ENVIRONMENT");
    if (string.IsNullOrEmpty(targetEnvironment))
    {
        Console.Error.WriteLine("NEWS_DEPLOYMENT_ENVIRONMENT must be set");
        return 1;
    }

    try
    {
        NewsSeeder.ValidateEnvironments(environment, targetEnvironment);

        await using var context = new NewsDbContext(NewsDbContextOptionsFactory.Build(databaseUrl));
        var repository = new NewsRepository(context);
        var count = await NewsSeeder.SeedNewsAsync(repository, environment, targetEnvironment);
        Console.WriteLine($"Seeded {count} {environment} News records.");
        return 0;
    }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

public partial class Program;
