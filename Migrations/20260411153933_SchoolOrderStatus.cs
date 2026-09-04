using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStockIQ.Migrations
{
    /// <inheritdoc />
    public partial class SchoolOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAtUtc",
                table: "SchoolOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionNote",
                table: "SchoolOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SchoolOrders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Legacy rows already had stock deducted at placement time — treat them as accepted.
            migrationBuilder.Sql(
                "UPDATE SchoolOrders SET Status = 1, DecidedAtUtc = CreatedAtUtc WHERE Status = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DecidedAtUtc",
                table: "SchoolOrders");

            migrationBuilder.DropColumn(
                name: "DecisionNote",
                table: "SchoolOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SchoolOrders");
        }
    }
}
