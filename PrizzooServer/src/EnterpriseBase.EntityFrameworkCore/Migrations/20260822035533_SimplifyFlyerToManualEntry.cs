using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseBase.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyFlyerToManualEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlyerItems");

            migrationBuilder.DropIndex(
                name: "IX_Flyers_StoreId_Status",
                table: "Flyers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Flyers");

            migrationBuilder.AddColumn<Guid>(
                name: "FlyerId",
                table: "Prices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prices_FlyerId",
                table: "Prices",
                column: "FlyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Flyers_StoreId",
                table: "Flyers",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prices_Flyers_FlyerId",
                table: "Prices",
                column: "FlyerId",
                principalTable: "Flyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prices_Flyers_FlyerId",
                table: "Prices");

            migrationBuilder.DropIndex(
                name: "IX_Prices_FlyerId",
                table: "Prices");

            migrationBuilder.DropIndex(
                name: "IX_Flyers_StoreId",
                table: "Flyers");

            migrationBuilder.DropColumn(
                name: "FlyerId",
                table: "Prices");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Flyers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FlyerItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchedProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ExtractedName = table.Column<string>(type: "text", nullable: true),
                    ExtractedPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    HeightPct = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    ModerationNote = table.Column<string>(type: "text", nullable: true),
                    ResultingPriceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WidthPct = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    XPct = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    YPct = table.Column<decimal>(type: "numeric(6,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlyerItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlyerItems_Flyers_FlyerId",
                        column: x => x.FlyerId,
                        principalTable: "Flyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlyerItems_Products_MatchedProductId",
                        column: x => x.MatchedProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flyers_StoreId_Status",
                table: "Flyers",
                columns: new[] { "StoreId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FlyerItems_FlyerId",
                table: "FlyerItems",
                column: "FlyerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlyerItems_MatchedProductId",
                table: "FlyerItems",
                column: "MatchedProductId");
        }
    }
}
