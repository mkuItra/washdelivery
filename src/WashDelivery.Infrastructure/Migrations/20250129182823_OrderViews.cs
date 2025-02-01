using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WashDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LaundryOrderViews",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    OrderId = table.Column<string>(type: "TEXT", nullable: false),
                    LaundryId = table.Column<string>(type: "TEXT", nullable: false),
                    FirstSeenAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HasResponded = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaundryOrderViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LaundryOrderViews_Laundries_LaundryId",
                        column: x => x.LaundryId,
                        principalTable: "Laundries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LaundryOrderViews_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LaundryOrderViews_LaundryId",
                table: "LaundryOrderViews",
                column: "LaundryId");

            migrationBuilder.CreateIndex(
                name: "IX_LaundryOrderViews_OrderId_LaundryId",
                table: "LaundryOrderViews",
                columns: new[] { "OrderId", "LaundryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LaundryOrderViews");
        }
    }
}
