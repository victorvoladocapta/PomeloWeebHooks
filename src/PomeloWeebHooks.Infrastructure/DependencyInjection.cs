using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PomeloWeebHooks.Application.CardEvents;
using PomeloWeebHooks.Application.CreditLineStatus;
using PomeloWeebHooks.Application.Statements;
using PomeloWeebHooks.Application.Transactions;
using PomeloWeebHooks.Application.Operations;
using PomeloWeebHooks.Application.Delinquency;
using PomeloWeebHooks.Application.Security;
using PomeloWeebHooks.Infrastructure.Persistence;
using PomeloWeebHooks.Infrastructure.Pomelo;

namespace PomeloWeebHooks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:PostgreSql es requerido.");

        services.Configure<PomeloWebhookOptions>(configuration.GetSection(PomeloWebhookOptions.SectionName));
        services.AddDbContext<WebhookDbContext>(options =>
            options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());
        services.AddScoped<IPomeloCardEventStore, PomeloCardEventStore>();
        services.AddScoped<IPomeloCreditLineStatusStore, PomeloCreditLineStatusStore>();
        services.AddScoped<IPomeloStatementCreatedStore, PomeloStatementCreatedStore>();
        services.AddScoped<IPomeloProcessedTransactionStore, PomeloProcessedTransactionStore>();
        services.AddScoped<IPomeloRevertedOperationStore, PomeloRevertedOperationStore>();
        services.AddScoped<IPomeloDelinquencyStore, PomeloDelinquencyStore>();
        services.AddSingleton<IPomeloWebhookVerifier, PomeloWebhookVerifier>();
        return services;
    }
}
