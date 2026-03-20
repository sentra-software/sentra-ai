using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiQueryAuditLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_query_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdentityUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Question = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    GeneratedSql = table.Column<string>(type: "text", nullable: true),
                    RowCount = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_query_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_query_audit_logs_CreatedAtUtc",
                table: "ai_query_audit_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ai_query_audit_logs_DataSourceId",
                table: "ai_query_audit_logs",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_query_audit_logs_IdentityUserId",
                table: "ai_query_audit_logs",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_query_audit_logs_TenantId",
                table: "ai_query_audit_logs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_query_audit_logs");
        }
    }
}
