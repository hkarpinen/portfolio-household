using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DemoMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "demo_seed_completed_at",
                schema: "household",
                table: "user_projections",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_demo",
                schema: "household",
                table: "user_projections",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "demo_seed_completed_at",
                schema: "household",
                table: "user_projections");

            migrationBuilder.DropColumn(
                name: "is_demo",
                schema: "household",
                table: "user_projections");
        }
    }
}
