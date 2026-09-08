using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PomeloWeebHooks.Infrastructure.Persistence;

namespace PomeloWeebHooks.API;

public sealed class WebhookDbContextFactory : IDesignTimeDbContextFactory<WebhookDbContext>
{
    public WebhookDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSql")
            ?? "Host=localhost;Port=5432;Database=captacard;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<WebhookDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new WebhookDbContext(options);
    }
}
