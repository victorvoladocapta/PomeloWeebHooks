using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PomeloWeebHooks.Application.CardEvents;
using PomeloWeebHooks.Application.Chargebacks;
using PomeloWeebHooks.Application.CreditLineStatus;
using PomeloWeebHooks.Application.Delinquency;
using PomeloWeebHooks.Application.InboundEvents;
using PomeloWeebHooks.Application.InterestCharges;
using PomeloWeebHooks.Application.Operations;
using PomeloWeebHooks.Application.Presentments;
using PomeloWeebHooks.Application.Shipping;
using PomeloWeebHooks.Application.Statements;
using PomeloWeebHooks.Application.TransactionNotifications;
using PomeloWeebHooks.Application.Transactions;
using PomeloWeebHooks.Application.UserStatus;
using PomeloWeebHooks.Infrastructure;
using PomeloWeebHooks.Infrastructure.Configuration;
using PomeloWeebHooks.Infrastructure.Persistence;
using PomeloWeebHooks.Infrastructure.Pomelo;

var builder = WebApplication.CreateBuilder(args);

// Both readers are no-ops off AWS: without the ARN variables they return null
// and local development keeps working from appsettings exactly as the README
// describes. They are added after the default sources so the values the stack
// supplies win over anything checked into the image.
var awsDatabase = await AwsDatabaseConfiguration.BuildAsync();
if (awsDatabase is not null)
    builder.Configuration.AddInMemoryCollection(awsDatabase);

var awsIntegrations = await AwsIntegrationsConfiguration.BuildAsync();
if (awsIntegrations is not null)
    builder.Configuration.AddInMemoryCollection(awsIntegrations);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<PomeloCardEventService>();
builder.Services.AddScoped<PomeloCreditLineStatusService>();
builder.Services.AddScoped<PomeloStatementCreatedService>();
builder.Services.AddScoped<PomeloProcessedTransactionService>();
builder.Services.AddScoped<PomeloRevertedOperationService>();
builder.Services.AddScoped<PomeloDelinquencyService>();
builder.Services.AddScoped<PomeloInboundEventService>();
builder.Services.AddScoped<PomeloTransactionNotificationService>();
builder.Services.AddScoped<PomeloPresentmentService>();
builder.Services.AddScoped<PomeloStatementOpenedService>();
builder.Services.AddScoped<PomeloInterestChargeService>();
builder.Services.AddScoped<PomeloUserStatusService>();
builder.Services.AddScoped<PomeloShippingService>();
builder.Services.AddScoped<PomeloChargebackService>();
builder.Services.AddInfrastructure(builder.Configuration);

// The verifier fails closed, which is right, but it fails closed one request at
// a time: without a secret every webhook is answered 401, Pomelo retries for a
// while and gives up, and nothing in this service's logs looks like an error
// because rejecting unsigned requests is exactly its job. Refusing to start is
// the only version of that failure anybody notices.
var pomeloOptions = builder.Configuration
    .GetSection(PomeloWebhookOptions.SectionName)
    .Get<PomeloWebhookOptions>() ?? new PomeloWebhookOptions();

if (!builder.Environment.IsDevelopment()
    && (string.IsNullOrWhiteSpace(pomeloOptions.ApiKey)
        || string.IsNullOrWhiteSpace(pomeloOptions.ApiSecret)))
{
    throw new InvalidOperationException(
        "Pomelo:ApiKey y Pomelo:ApiSecret son obligatorios fuera de Development. " +
        "En AWS llegan desde el secreto que INTEGRATIONS_SECRET_ARN identifica, " +
        "con las claves Pomelo__ApiKey y Pomelo__ApiSecret. Sin ellos el servicio " +
        "rechazaria todos los webhooks en silencio.");
}

var app = builder.Build();

if (app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WebhookDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Deliberately no UseHttpsRedirection. CloudFront already forces HTTPS at the
// edge, and the load balancer terminates TLS before speaking plain HTTP to this
// container. Left in place, the redirect middleware sees a non-TLS request and
// answers every webhook with a 307 instead of a 200 -- and a redirected POST is
// one many clients do not repeat with the body attached.
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

public partial class Program;
