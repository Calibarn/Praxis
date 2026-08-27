using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Praxis.Backend.Application.Repositories.Interfaces;

namespace Praxis.Backend.Tests.Api;

/// <summary>
/// A test host with the real DB configuration replaced by an unused dummy URL
/// (never connected to) and <see cref="INewsRepository"/> swapped for a fake —
/// the .NET analogue of FastAPI's dependency_overrides used in the Python tests.
/// </summary>
public sealed class NewsApiTestFactory(Func<INewsRepository> repositoryFactory) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("NEWS_DATABASE_URL", "mysql+asyncmy://unused:unused@unused/unused");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<INewsRepository>();
            services.AddScoped<INewsRepository>(_ => repositoryFactory());
        });
    }
}
