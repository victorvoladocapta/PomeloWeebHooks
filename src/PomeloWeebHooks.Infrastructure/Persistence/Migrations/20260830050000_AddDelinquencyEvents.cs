using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WebhookDbContext)), Migration("20260830050000_AddDelinquencyEvents")]
public partial class AddDelinquencyEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE TABLE webhooks.pomelo_delinquency_event (
          id uuid NOT NULL PRIMARY KEY, idempotency_key varchar(128) NOT NULL, event_id varchar(64) NOT NULL,
          user_id varchar(128) NOT NULL, credit_line_id varchar(128) NOT NULL, effective_at varchar(64) NOT NULL,
          payload_json jsonb NOT NULL, received_at timestamptz NOT NULL, processed_at timestamptz NOT NULL);
        CREATE UNIQUE INDEX ix_pomelo_delinquency_event_idempotency_key ON webhooks.pomelo_delinquency_event (idempotency_key);
        CREATE INDEX ix_pomelo_delinquency_event_user_id ON webhooks.pomelo_delinquency_event (user_id);
        CREATE INDEX ix_pomelo_delinquency_event_credit_line_id ON webhooks.pomelo_delinquency_event (credit_line_id);
        """);
    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "pomelo_delinquency_event", schema: "webhooks");
}
