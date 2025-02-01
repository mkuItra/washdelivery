using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WashDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RejectedOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAssigned",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "RejectedOrders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CourierId = table.Column<string>(type: "TEXT", nullable: false),
                    OrderId = table.Column<string>(type: "TEXT", nullable: false),
                    RejectedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectedOrders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RejectedOrders");

            migrationBuilder.AddColumn<bool>(
                name: "IsAssigned",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);
        }
    }
}
