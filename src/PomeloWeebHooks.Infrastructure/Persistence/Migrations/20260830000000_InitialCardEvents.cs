using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations;

[DbContext(typeof(WebhookDbContext))]
[Migration("20260830000000_InitialCardEvents")]
public partial class InitialCardEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "webhooks");

        migrationBuilder.CreateTable(
            name: "pomelo_card_event",
            schema: "webhooks",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                event_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                pomelo_card_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                pomelo_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                event_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                card_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                related_card_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                pomelo_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                payload_json = table.Column<string>(type: "jsonb", nullable: false),
                status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table => table.PrimaryKey("pk_pomelo_card_event", x => x.id));

        migrationBuilder.CreateIndex(
            name: "ix_pomelo_card_event_idempotency_key",
            schema: "webhooks",
            table: "pomelo_card_event",
            column: "idempotency_key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_pomelo_card_event_pomelo_card_id",
            schema: "webhooks",
            table: "pomelo_card_event",
            column: "pomelo_card_id");

        migrationBuilder.CreateIndex(
            name: "ix_pomelo_card_event_received_at",
            schema: "webhooks",
            table: "pomelo_card_event",
            column: "received_at");
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "pomelo_card_event", schema: "webhooks");
}
