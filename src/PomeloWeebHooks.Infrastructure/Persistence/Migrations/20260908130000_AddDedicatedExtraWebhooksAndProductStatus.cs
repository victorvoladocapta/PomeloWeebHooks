using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WebhookDbContext))]
[Migration("20260908130000_AddDedicatedExtraWebhooksAndProductStatus")]
public partial class AddDedicatedExtraWebhooksAndProductStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE webhooks.pomelo_card_event
                ADD COLUMN IF NOT EXISTS product_status character varying(32) NOT NULL DEFAULT 'Pending',
                ADD COLUMN IF NOT EXISTS product_processed_at timestamp with time zone,
                ADD COLUMN IF NOT EXISTS product_error character varying(1024);

            ALTER TABLE webhooks.pomelo_credit_line_status_event
                ADD COLUMN IF NOT EXISTS product_status character varying(32) NOT NULL DEFAULT 'Pending',
                ADD COLUMN IF NOT EXISTS product_processed_at timestamp with time zone,
                ADD COLUMN IF NOT EXISTS product_error character varying(1024);

            ALTER TABLE webhooks.pomelo_statement_created_event
                ADD COLUMN IF NOT EXISTS product_status character varying(32) NOT NULL DEFAULT 'Pending',
                ADD COLUMN IF NOT EXISTS product_processed_at timestamp with time zone,
                ADD COLUMN IF NOT EXISTS product_error character varying(1024);

            ALTER TABLE webhooks.pomelo_processed_transaction_event
                ADD COLUMN IF NOT EXISTS product_status character varying(32) NOT NULL DEFAULT 'Pending',
                ADD COLUMN IF NOT EXISTS product_processed_at timestamp with time zone,
                ADD COLUMN IF NOT EXISTS product_error character varying(1024);

            ALTER TABLE webhooks.pomelo_reverted_operation_event
                ADD COLUMN IF NOT EXISTS product_status character varying(32) NOT NULL DEFAULT 'Pending',
                ADD COLUMN IF NOT EXISTS product_processed_at timestamp with time zone,
                ADD COLUMN IF NOT EXISTS product_error character varying(1024);

            ALTER TABLE webhooks.pomelo_delinquency_event
                ADD COLUMN IF NOT EXISTS product_status character varying(32) NOT NULL DEFAULT 'Pending',
                ADD COLUMN IF NOT EXISTS product_processed_at timestamp with time zone,
                ADD COLUMN IF NOT EXISTS product_error character varying(1024);

            CREATE TABLE IF NOT EXISTS webhooks.pomelo_transaction_notification_event (
                id uuid PRIMARY KEY,
                idempotency_key character varying(128) NOT NULL,
                event_id character varying(64) NOT NULL,
                variant character varying(16) NOT NULL DEFAULT 'network',
                transaction_id character varying(128),
                status character varying(64),
                status_detail character varying(128),
                pomelo_user_id character varying(128),
                pomelo_card_id character varying(128),
                merchant_name character varying(256),
                local_amount_total character varying(64),
                local_amount_currency character varying(8),
                payload_json jsonb NOT NULL,
                received_at timestamp with time zone NOT NULL,
                product_status character varying(32) NOT NULL DEFAULT 'Pending',
                product_processed_at timestamp with time zone,
                product_error character varying(1024)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_pomelo_transaction_notification_event_idempotency_key
                ON webhooks.pomelo_transaction_notification_event (idempotency_key);
            CREATE INDEX IF NOT EXISTS ix_pomelo_transaction_notification_event_product_status
                ON webhooks.pomelo_transaction_notification_event (product_status);

            CREATE TABLE IF NOT EXISTS webhooks.pomelo_presentment_event (
                id uuid PRIMARY KEY,
                idempotency_key character varying(128) NOT NULL,
                event_id character varying(64) NOT NULL,
                presentment_id character varying(128),
                status character varying(64),
                pomelo_user_id character varying(128),
                pomelo_card_id character varying(128),
                original_transaction_id character varying(128),
                amount_total character varying(64),
                amount_currency character varying(8),
                payload_json jsonb NOT NULL,
                received_at timestamp with time zone NOT NULL,
                product_status character varying(32) NOT NULL DEFAULT 'Pending',
                product_processed_at timestamp with time zone,
                product_error character varying(1024)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_pomelo_presentment_event_idempotency_key
                ON webhooks.pomelo_presentment_event (idempotency_key);
            CREATE INDEX IF NOT EXISTS ix_pomelo_presentment_event_product_status
                ON webhooks.pomelo_presentment_event (product_status);

            CREATE TABLE IF NOT EXISTS webhooks.pomelo_statement_opened_event (
                id uuid PRIMARY KEY,
                idempotency_key character varying(128) NOT NULL,
                event_id character varying(64) NOT NULL,
                statement_id character varying(128) NOT NULL,
                credit_line_id character varying(128) NOT NULL,
                start_date character varying(32),
                closing_date character varying(32),
                payload_json jsonb NOT NULL,
                received_at timestamp with time zone NOT NULL,
                product_status character varying(32) NOT NULL DEFAULT 'Pending',
                product_processed_at timestamp with time zone,
                product_error character varying(1024)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_pomelo_statement_opened_event_idempotency_key
                ON webhooks.pomelo_statement_opened_event (idempotency_key);
            CREATE INDEX IF NOT EXISTS ix_pomelo_statement_opened_event_product_status
                ON webhooks.pomelo_statement_opened_event (product_status);

            CREATE TABLE IF NOT EXISTS webhooks.pomelo_interest_charge_event (
                id uuid PRIMARY KEY,
                idempotency_key character varying(128) NOT NULL,
                event_id character varying(64) NOT NULL,
                interest_id character varying(128) NOT NULL,
                pomelo_user_id character varying(128),
                credit_line_id character varying(128),
                debt_id character varying(128),
                origin character varying(64),
                type character varying(64),
                amount_total character varying(64),
                amount_currency character varying(8),
                effective_at character varying(64),
                payload_json jsonb NOT NULL,
                received_at timestamp with time zone NOT NULL,
                product_status character varying(32) NOT NULL DEFAULT 'Pending',
                product_processed_at timestamp with time zone,
                product_error character varying(1024)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_pomelo_interest_charge_event_idempotency_key
                ON webhooks.pomelo_interest_charge_event (idempotency_key);
            CREATE INDEX IF NOT EXISTS ix_pomelo_interest_charge_event_product_status
                ON webhooks.pomelo_interest_charge_event (product_status);

            CREATE TABLE IF NOT EXISTS webhooks.pomelo_user_status_event (
                id uuid PRIMARY KEY,
                idempotency_key character varying(128) NOT NULL,
                event_id character varying(64) NOT NULL,
                pomelo_user_id character varying(128) NOT NULL,
                user_status character varying(64),
                user_status_reason character varying(128),
                payload_json jsonb NOT NULL,
                received_at timestamp with time zone NOT NULL,
                product_status character varying(32) NOT NULL DEFAULT 'Pending',
                product_processed_at timestamp with time zone,
                product_error character varying(1024)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_pomelo_user_status_event_idempotency_key
                ON webhooks.pomelo_user_status_event (idempotency_key);
            CREATE INDEX IF NOT EXISTS ix_pomelo_user_status_event_product_status
                ON webhooks.pomelo_user_status_event (product_status);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS webhooks.pomelo_user_status_event;
            DROP TABLE IF EXISTS webhooks.pomelo_interest_charge_event;
            DROP TABLE IF EXISTS webhooks.pomelo_statement_opened_event;
            DROP TABLE IF EXISTS webhooks.pomelo_presentment_event;
            DROP TABLE IF EXISTS webhooks.pomelo_transaction_notification_event;
            """);
    }
}
