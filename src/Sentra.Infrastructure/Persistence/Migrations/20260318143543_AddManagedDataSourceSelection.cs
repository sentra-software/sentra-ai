using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentra.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManagedDataSourceSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveDataSourceId",
                table: "identity_users",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveDataSourceId",
                table: "identity_users");
        }
    }
}
