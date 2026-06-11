using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CalendarBillSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "linked_expense_id",
                schema: "household",
                table: "calendar_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "recurrence_end_date",
                schema: "household",
                table: "calendar_events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "recurrence_frequency",
                schema: "household",
                table: "calendar_events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source",
                schema: "household",
                table: "calendar_events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_calendar_events_source_linked_expense_id",
                schema: "household",
                table: "calendar_events",
                columns: new[] { "source", "linked_expense_id" },
                unique: true,
                filter: "source = 1 AND linked_expense_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_calendar_events_source_linked_expense_id",
                schema: "household",
                table: "calendar_events");

            migrationBuilder.DropColumn(
                name: "linked_expense_id",
                schema: "household",
                table: "calendar_events");

            migrationBuilder.DropColumn(
                name: "recurrence_end_date",
                schema: "household",
                table: "calendar_events");

            migrationBuilder.DropColumn(
                name: "recurrence_frequency",
                schema: "household",
                table: "calendar_events");

            migrationBuilder.DropColumn(
                name: "source",
                schema: "household",
                table: "calendar_events");
        }
    }
}
