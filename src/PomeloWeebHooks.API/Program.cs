using Microsoft.EntityFrameworkCore;
using PomeloWeebHooks.Application.CardEvents;
using PomeloWeebHooks.Application.CreditLineStatus;
using PomeloWeebHooks.Application.Statements;
using PomeloWeebHooks.Application.Transactions;
using PomeloWeebHooks.Application.Operations;
using PomeloWeebHooks.Application.Delinquency;
using PomeloWeebHooks.Infrastructure;
using PomeloWeebHooks.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<PomeloCardEventService>();
builder.Services.AddScoped<PomeloCreditLineStatusService>();
builder.Services.AddScoped<PomeloStatementCreatedService>();
builder.Services.AddScoped<PomeloProcessedTransactionService>();
builder.Services.AddScoped<PomeloRevertedOperationService>();
builder.Services.AddScoped<PomeloDelinquencyService>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<WebhookDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

public partial class Program;
