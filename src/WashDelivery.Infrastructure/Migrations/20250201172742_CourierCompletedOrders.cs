using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WashDelivery.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CourierCompletedOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourierCompletedOrders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CourierId = table.Column<string>(type: "TEXT", nullable: false),
                    OrderId = table.Column<string>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourierCompletedOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourierCompletedOrders_AspNetUsers_CourierId",
                        column: x => x.CourierId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourierCompletedOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourierCompletedOrders_CourierId",
                table: "CourierCompletedOrders",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "IX_CourierCompletedOrders_OrderId",
                table: "CourierCompletedOrders",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourierCompletedOrders");
        }
    }
}
