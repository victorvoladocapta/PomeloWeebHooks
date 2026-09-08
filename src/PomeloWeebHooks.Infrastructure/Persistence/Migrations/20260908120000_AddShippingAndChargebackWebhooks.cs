using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PomeloWeebHooks.Infrastructure.Persistence;

#nullable disable

namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WebhookDbContext))]
[Migration("20260908120000_AddShippingAndChargebackWebhooks")]
public partial class AddShippingAndChargebackWebhooks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS webhooks.pomelo_shipping_event (
                id uuid NOT NULL,
                idempotency_key character varying(128) NOT NULL,
                event_id character varying(64) NOT NULL,
                shipment_id character varying(128) NOT NULL,
                status character varying(64),
                status_detail character varying(128),
                request_status character varying(64),
                payload_json jsonb NOT NULL,
                received_at timestamp with time zone NOT NULL,
                product_status character varying(32) NOT NULL DEFAULT 'Pending',
                product_processed_at timestamp with time zone,
                product_error character varying(1024),
                CONSTRAINT pk_pomelo_shipping_event PRIMARY KEY (id)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_pomelo_shipping_event_idempotency_key
                ON webhooks.pomelo_shipping_event (idempotency_key);
            CREATE INDEX IF NOT EXISTS ix_pomelo_shipping_event_shipment_id
                ON webhooks.pomelo_shipping_event (shipment_id);
            CREATE INDEX IF NOT EXISTS ix_pomelo_shipping_event_product_status
                ON webhooks.pomelo_shipping_event (product_status);

            CREATE TABLE IF NOT EXISTS webhooks.pomelo_chargeback_event (
                id uuid NOT NULL,
                idempotency_key character varying(128) NOT NULL,
                event_id character varying(64) NOT NULL,
                chargeback_id character varying(128) NOT NULL,
                transaction_id character varying(128) NOT NULL,
                status character varying(64),
                status_ticket character varying(64),
                amount character varying(64),
                currency character varying(8),
                payload_json jsonb NOT NULL,
                received_at timestamp with time zone NOT NULL,
                product_status character varying(32) NOT NULL DEFAULT 'Pending',
                product_processed_at timestamp with time zone,
                product_error character varying(1024),
                CONSTRAINT pk_pomelo_chargeback_event PRIMARY KEY (id)
            );
            CREATE UNIQUE INDEX IF NOT EXISTS ix_pomelo_chargeback_event_idempotency_key
                ON webhooks.pomelo_chargeback_event (idempotency_key);
            CREATE INDEX IF NOT EXISTS ix_pomelo_chargeback_event_transaction_id
                ON webhooks.pomelo_chargeback_event (transaction_id);
            CREATE INDEX IF NOT EXISTS ix_pomelo_chargeback_event_product_status
                ON webhooks.pomelo_chargeback_event (product_status);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS webhooks.pomelo_chargeback_event;
            DROP TABLE IF EXISTS webhooks.pomelo_shipping_event;
            """);
    }
}
