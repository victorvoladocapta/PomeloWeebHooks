using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PomeloWeebHooks.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "webhooks");

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
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pomelo_card_event", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pomelo_credit_line_status_event",
                schema: "webhooks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    event_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    credit_line_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    reason = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pomelo_credit_line_status_event", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pomelo_delinquency_event",
                schema: "webhooks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    event_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    credit_line_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    effective_at = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pomelo_delinquency_event", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pomelo_processed_transaction_event",
                schema: "webhooks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    event_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status_detail = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    credit_line_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    card_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    card_last_four = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    merchant_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    merchant_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    installments_quantity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    transaction_date_time = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    local_amount_total = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    local_amount_currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pomelo_processed_transaction_event", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pomelo_reverted_operation_event",
                schema: "webhooks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    event_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    operation_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    credit_line_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    card_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    card_last_four = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    user_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    merchant_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    merchant_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    installments_quantity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    reverted_date_time = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    local_amount_total = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    local_amount_currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pomelo_reverted_operation_event", x => x.id);
                });

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
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pomelo_statement_created_event", x => x.id);
                });

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

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_credit_line_status_event_credit_line_id",
                schema: "webhooks",
                table: "pomelo_credit_line_status_event",
                column: "credit_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_credit_line_status_event_idempotency_key",
                schema: "webhooks",
                table: "pomelo_credit_line_status_event",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_credit_line_status_event_received_at",
                schema: "webhooks",
                table: "pomelo_credit_line_status_event",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_delinquency_event_credit_line_id",
                schema: "webhooks",
                table: "pomelo_delinquency_event",
                column: "credit_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_delinquency_event_idempotency_key",
                schema: "webhooks",
                table: "pomelo_delinquency_event",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_delinquency_event_user_id",
                schema: "webhooks",
                table: "pomelo_delinquency_event",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_processed_transaction_event_credit_line_id",
                schema: "webhooks",
                table: "pomelo_processed_transaction_event",
                column: "credit_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_processed_transaction_event_idempotency_key",
                schema: "webhooks",
                table: "pomelo_processed_transaction_event",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_processed_transaction_event_transaction_id",
                schema: "webhooks",
                table: "pomelo_processed_transaction_event",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_reverted_operation_event_credit_line_id",
                schema: "webhooks",
                table: "pomelo_reverted_operation_event",
                column: "credit_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_reverted_operation_event_idempotency_key",
                schema: "webhooks",
                table: "pomelo_reverted_operation_event",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pomelo_reverted_operation_event_operation_id",
                schema: "webhooks",
                table: "pomelo_reverted_operation_event",
                column: "operation_id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pomelo_card_event",
                schema: "webhooks");

            migrationBuilder.DropTable(
                name: "pomelo_credit_line_status_event",
                schema: "webhooks");

            migrationBuilder.DropTable(
                name: "pomelo_delinquency_event",
                schema: "webhooks");

            migrationBuilder.DropTable(
                name: "pomelo_processed_transaction_event",
                schema: "webhooks");

            migrationBuilder.DropTable(
                name: "pomelo_reverted_operation_event",
                schema: "webhooks");

            migrationBuilder.DropTable(
                name: "pomelo_statement_created_event",
                schema: "webhooks");
        }
    }
}
