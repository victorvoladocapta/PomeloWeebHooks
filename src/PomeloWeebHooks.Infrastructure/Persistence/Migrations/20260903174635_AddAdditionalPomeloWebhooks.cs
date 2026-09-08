using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalPomeloWebhooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pomelo_inbound_event",
                schema: "webhooks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    event_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    resource_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    related_resource_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    pomelo_user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    status = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pomelo_inbound_event", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_inbound_event_kind_idempotency_key",
                schema: "webhooks",
                table: "pomelo_inbound_event",
                columns: new[] { "kind", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_inbound_event_kind_resource_id",
                schema: "webhooks",
                table: "pomelo_inbound_event",
                columns: new[] { "kind", "resource_id" });

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_inbound_event_pomelo_user_id",
                schema: "webhooks",
                table: "pomelo_inbound_event",
                column: "pomelo_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_inbound_event_received_at",
                schema: "webhooks",
                table: "pomelo_inbound_event",
                column: "received_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pomelo_inbound_event",
                schema: "webhooks");
        }
    }
}
