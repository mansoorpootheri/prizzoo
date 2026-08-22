using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseBase.Migrations
{
    /// <inheritdoc />
    public partial class AddFlyerAndFlyerItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flyers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StoreId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UploadedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flyers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flyers_AppBinaryObjects_ImageId",
                        column: x => x.ImageId,
                        principalTable: "AppBinaryObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flyers_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlyerItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    XPct = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    YPct = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    WidthPct = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    HeightPct = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    ExtractedName = table.Column<string>(type: "text", nullable: true),
                    ExtractedPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MatchedProductId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ModerationNote = table.Column<string>(type: "text", nullable: true),
                    ResultingPriceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp", nullable: true)
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
                name: "IX_FlyerItems_FlyerId",
                table: "FlyerItems",
                column: "FlyerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlyerItems_MatchedProductId",
                table: "FlyerItems",
                column: "MatchedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Flyers_ImageId",
                table: "Flyers",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Flyers_StoreId_Status",
                table: "Flyers",
                columns: new[] { "StoreId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlyerItems");

            migrationBuilder.DropTable(
                name: "Flyers");
        }
    }
}
