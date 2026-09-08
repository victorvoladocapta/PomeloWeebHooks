using Microsoft.EntityFrameworkCore;
using PomeloWeebHooks.Core.Entities;

namespace PomeloWeebHooks.Infrastructure.Persistence;

public sealed class WebhookDbContext(DbContextOptions<WebhookDbContext> options) : DbContext(options)
{
    public DbSet<PomeloCardEvent> PomeloCardEvents => Set<PomeloCardEvent>();
    public DbSet<PomeloCreditLineStatusEvent> PomeloCreditLineStatusEvents => Set<PomeloCreditLineStatusEvent>();
    public DbSet<PomeloStatementCreatedEvent> PomeloStatementCreatedEvents => Set<PomeloStatementCreatedEvent>();
    public DbSet<PomeloProcessedTransactionEvent> PomeloProcessedTransactionEvents => Set<PomeloProcessedTransactionEvent>();
    public DbSet<PomeloRevertedOperationEvent> PomeloRevertedOperationEvents => Set<PomeloRevertedOperationEvent>();
    public DbSet<PomeloDelinquencyEvent> PomeloDelinquencyEvents => Set<PomeloDelinquencyEvent>();
    public DbSet<PomeloInboundEvent> PomeloInboundEvents => Set<PomeloInboundEvent>();
    public DbSet<PomeloShippingEvent> PomeloShippingEvents => Set<PomeloShippingEvent>();
    public DbSet<PomeloChargebackEvent> PomeloChargebackEvents => Set<PomeloChargebackEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PomeloCardEvent>(entity =>
        {
            entity.ToTable("pomelo_card_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PomeloCardId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PomeloUserId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CardType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.RelatedCardId).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.PomeloCardId);
            entity.HasIndex(x => x.ReceivedAt);
        });

        modelBuilder.Entity<PomeloCreditLineStatusEvent>(entity =>
        {
            entity.ToTable("pomelo_credit_line_status_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreditLineId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.CreditLineId);
            entity.HasIndex(x => x.ReceivedAt);
        });

        modelBuilder.Entity<PomeloStatementCreatedEvent>(entity =>
        {
            entity.ToTable("pomelo_statement_created_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatementId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreditLineId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.StatementId);
            entity.HasIndex(x => x.CreditLineId);
        });

        modelBuilder.Entity<PomeloProcessedTransactionEvent>(entity =>
        {
            entity.ToTable("pomelo_processed_transaction_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TransactionId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StatusDetail).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreditLineId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CardId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CardLastFour).HasMaxLength(4);
            entity.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MerchantId).HasMaxLength(128);
            entity.Property(x => x.MerchantName).HasMaxLength(256);
            entity.Property(x => x.InstallmentsQuantity).HasMaxLength(16);
            entity.Property(x => x.TransactionDateTime).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LocalAmountTotal).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LocalAmountCurrency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.TransactionId);
            entity.HasIndex(x => x.CreditLineId);
        });

        modelBuilder.Entity<PomeloRevertedOperationEvent>(entity =>
        {
            entity.ToTable("pomelo_reverted_operation_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OperationId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreditLineId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CardId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CardLastFour).HasMaxLength(4);
            entity.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MerchantId).HasMaxLength(128);
            entity.Property(x => x.MerchantName).HasMaxLength(256);
            entity.Property(x => x.InstallmentsQuantity).HasMaxLength(16);
            entity.Property(x => x.RevertedDateTime).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LocalAmountTotal).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LocalAmountCurrency).HasMaxLength(8).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.OperationId);
            entity.HasIndex(x => x.CreditLineId);
        });

        modelBuilder.Entity<PomeloDelinquencyEvent>(entity =>
        {
            entity.ToTable("pomelo_delinquency_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreditLineId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EffectiveAt).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.CreditLineId);
        });

        modelBuilder.Entity<PomeloInboundEvent>(entity =>
        {
            entity.ToTable("pomelo_inbound_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Kind).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ResourceId).HasMaxLength(128);
            entity.Property(x => x.RelatedResourceId).HasMaxLength(128);
            entity.Property(x => x.PomeloUserId).HasMaxLength(128);
            entity.Property(x => x.Status).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => new { x.Kind, x.IdempotencyKey }).IsUnique();
            entity.HasIndex(x => new { x.Kind, x.ResourceId });
            entity.HasIndex(x => x.PomeloUserId);
            entity.HasIndex(x => x.ReceivedAt);
        });

        modelBuilder.Entity<PomeloShippingEvent>(entity =>
        {
            entity.ToTable("pomelo_shipping_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ShipmentId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64);
            entity.Property(x => x.StatusDetail).HasMaxLength(128);
            entity.Property(x => x.RequestStatus).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ProductStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProductError).HasMaxLength(1024);
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.ShipmentId);
            entity.HasIndex(x => x.ProductStatus);
        });

        modelBuilder.Entity<PomeloChargebackEvent>(entity =>
        {
            entity.ToTable("pomelo_chargeback_event", "webhooks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ChargebackId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.TransactionId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64);
            entity.Property(x => x.StatusTicket).HasMaxLength(64);
            entity.Property(x => x.Amount).HasMaxLength(64);
            entity.Property(x => x.Currency).HasMaxLength(8);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.ProductStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(x => x.ProductError).HasMaxLength(1024);
            entity.HasIndex(x => x.IdempotencyKey).IsUnique();
            entity.HasIndex(x => x.TransactionId);
            entity.HasIndex(x => x.ProductStatus);
        });
    }
}
