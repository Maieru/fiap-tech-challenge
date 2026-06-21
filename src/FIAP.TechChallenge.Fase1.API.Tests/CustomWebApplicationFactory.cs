using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace FIAP.TechChallenge.Fase1.API.Tests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"TestsDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder.UseEnvironment("Testing");
        _ = builder.ConfigureServices(services =>
        {
            _ = services.RemoveAll<DbContextOptions<AppDbContext>>();
            _ = services.RemoveAll<AppDbContext>();

            _ = services.AddDbContext<AppDbContext>(options =>
            {
                _ = options.UseInMemoryDatabase(_databaseName);
            });
        });

        _ = builder.ConfigureLogging(logging =>
        {
            _ = logging.ClearProviders();
            _ = logging.AddConsole();
        });
    }
}
