using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WebhookDbContext))]
[Migration("20260830020000_AddStatementCreatedEvents")]
public partial class AddStatementCreatedEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "pomelo_statement_created_event",
            schema: "webhooks",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                event_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                statement_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                credit_line_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                payload_json = table.Column<string>(type: "jsonb", nullable: false),
                received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table => table.PrimaryKey("pk_pomelo_statement_created_event", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_pomelo_statement_created_event_credit_line_id",
            schema: "webhooks",
            table: "pomelo_statement_created_event",
            column: "credit_line_id");

        migrationBuilder.CreateIndex(
            name: "ix_pomelo_statement_created_event_idempotency_key",
            schema: "webhooks",
            table: "pomelo_statement_created_event",
            column: "idempotency_key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_pomelo_statement_created_event_statement_id",
            schema: "webhooks",
            table: "pomelo_statement_created_event",
            column: "statement_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "pomelo_statement_created_event", schema: "webhooks");
}
