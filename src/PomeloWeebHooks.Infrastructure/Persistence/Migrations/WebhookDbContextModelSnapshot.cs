using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PomeloWeebHooks.Infrastructure.Persistence;

#nullable disable

namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WebhookDbContext))]
partial class WebhookDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.1");

        modelBuilder.Entity("PomeloWeebHooks.Core.Entities.PomeloCardEvent", entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uuid");
            entity.Property<string>("CardType").IsRequired().HasMaxLength(32).HasColumnType("character varying(32)");
            entity.Property<string>("EventId").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property<string>("EventType").IsRequired().HasMaxLength(32).HasColumnType("character varying(32)");
            entity.Property<string>("IdempotencyKey").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<string>("PayloadJson").IsRequired().HasColumnType("jsonb");
            entity.Property<string>("PomeloCardId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<DateTimeOffset>("PomeloUpdatedAt").HasColumnType("timestamp with time zone");
            entity.Property<string>("PomeloUserId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<DateTimeOffset?>("ProcessedAt").HasColumnType("timestamp with time zone");
            entity.Property<DateTimeOffset>("ReceivedAt").HasColumnType("timestamp with time zone");
            entity.Property<string>("RelatedCardId").HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<string>("Status").IsRequired().HasMaxLength(32).HasColumnType("character varying(32)");
            entity.HasKey("Id");
            entity.HasIndex("IdempotencyKey").IsUnique();
            entity.HasIndex("PomeloCardId");
            entity.HasIndex("ReceivedAt");
            entity.ToTable("pomelo_card_event", "webhooks");
        });

        modelBuilder.Entity("PomeloWeebHooks.Core.Entities.PomeloCreditLineStatusEvent", entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uuid");
            entity.Property<string>("CreditLineId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<string>("EventId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<string>("IdempotencyKey").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<string>("PayloadJson").IsRequired().HasColumnType("jsonb");
            entity.Property<DateTimeOffset>("ProcessedAt").HasColumnType("timestamp with time zone");
            entity.Property<DateTimeOffset>("ReceivedAt").HasColumnType("timestamp with time zone");
            entity.Property<string>("Reason").HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<string>("Status").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)");
            entity.HasKey("Id");
            entity.HasIndex("CreditLineId");
            entity.HasIndex("IdempotencyKey").IsUnique();
            entity.HasIndex("ReceivedAt");
            entity.ToTable("pomelo_credit_line_status_event", "webhooks");
        });

        modelBuilder.Entity("PomeloWeebHooks.Core.Entities.PomeloStatementCreatedEvent", entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uuid");
            entity.Property<string>("CreditLineId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<string>("EventId").IsRequired().HasMaxLength(64).HasColumnType("character varying(64)");
            entity.Property<string>("IdempotencyKey").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.Property<string>("PayloadJson").IsRequired().HasColumnType("jsonb");
            entity.Property<DateTimeOffset>("ProcessedAt").HasColumnType("timestamp with time zone");
            entity.Property<DateTimeOffset>("ReceivedAt").HasColumnType("timestamp with time zone");
            entity.Property<string>("StatementId").IsRequired().HasMaxLength(128).HasColumnType("character varying(128)");
            entity.HasKey("Id");
            entity.HasIndex("CreditLineId");
            entity.HasIndex("IdempotencyKey").IsUnique();
            entity.HasIndex("StatementId");
            entity.ToTable("pomelo_statement_created_event", "webhooks");
        });
    }
}
