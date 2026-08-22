using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseBase.Migrations
{
    /// <inheritdoc />
    public partial class AddFlyerProductAndRemovePriceFlyerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlyerProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FlyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FlyerProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlyerProducts_Flyers_FlyerId",
                        column: x => x.FlyerId,
                        principalTable: "Flyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlyerProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlyerProducts_FlyerId_ProductId",
                table: "FlyerProducts",
                columns: new[] { "FlyerId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlyerProducts_ProductId",
                table: "FlyerProducts",
                column: "ProductId");

            // Data cleanup: flyer items used to be written as their own Price
            // row (Source = 5 = the now-removed PriceSource.Flyer) alongside
            // whatever manually-entered price a product already had at that
            // store - the exact duplication this migration's app-layer
            // change (FlyerAppService no longer inserting a Price for an
            // already-priced product) fixes going forward. Remove the
            // historical duplicates too, not just prevent new ones.
            migrationBuilder.Sql(@"DELETE FROM ""Prices"" WHERE ""Source"" = 5;");

            migrationBuilder.DropForeignKey(
                name: "FK_Prices_Flyers_FlyerId",
                table: "Prices");

            migrationBuilder.DropIndex(
                name: "IX_Prices_FlyerId",
                table: "Prices");

            migrationBuilder.DropColumn(
                name: "FlyerId",
                table: "Prices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlyerProducts");

            migrationBuilder.AddColumn<Guid>(
                name: "FlyerId",
                table: "Prices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prices_FlyerId",
                table: "Prices",
                column: "FlyerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prices_Flyers_FlyerId",
                table: "Prices",
                column: "FlyerId",
                principalTable: "Flyers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
