using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WebhookDbContext))]
[Migration("20260830030000_AddProcessedTransactionEvents")]
public partial class AddProcessedTransactionEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE webhooks.pomelo_processed_transaction_event (
            id uuid NOT NULL PRIMARY KEY,
            idempotency_key varchar(128) NOT NULL,
            event_id varchar(64) NOT NULL,
            transaction_id varchar(128) NOT NULL,
            status varchar(64) NOT NULL,
            status_detail varchar(128) NOT NULL,
            credit_line_id varchar(128) NOT NULL,
            card_id varchar(128) NOT NULL,
            card_last_four varchar(4),
            user_id varchar(128) NOT NULL,
            merchant_id varchar(128),
            merchant_name varchar(256),
            installments_quantity varchar(16),
            transaction_date_time varchar(64) NOT NULL,
            local_amount_total varchar(64) NOT NULL,
            local_amount_currency varchar(8) NOT NULL,
            payload_json jsonb NOT NULL,
            received_at timestamptz NOT NULL,
            processed_at timestamptz NOT NULL
        );
        CREATE UNIQUE INDEX ix_pomelo_processed_transaction_event_idempotency_key ON webhooks.pomelo_processed_transaction_event (idempotency_key);
        CREATE INDEX ix_pomelo_processed_transaction_event_transaction_id ON webhooks.pomelo_processed_transaction_event (transaction_id);
        CREATE INDEX ix_pomelo_processed_transaction_event_credit_line_id ON webhooks.pomelo_processed_transaction_event (credit_line_id);
        """);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "pomelo_processed_transaction_event", schema: "webhooks");
}
