using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnterpriseBase.Migrations
{
    /// <inheritdoc />
    public partial class SyncStoreCoordinatesFromLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // One-time data fix: Store.Latitude/Longitude used to be captured
            // independently of the store's picked Location. Now that a
            // store's coordinates always come from its Location (see
            // StoreAppService.ResolveLocationAsync), bring every existing
            // store in line with its Location's current coordinates. Stores
            // whose Location has no coordinates yet are left untouched -
            // there's nothing better to derive, and they'll simply be
            // un-editable until an admin captures that Location's
            // coordinates (Admin > Locations > "use my current location").
            migrationBuilder.Sql(@"
                UPDATE ""Stores"" AS s
                SET ""Latitude""  = l.""Latitude"",
                    ""Longitude"" = l.""Longitude""
                FROM ""Locations"" AS l
                WHERE s.""LocationId"" = l.""Id""
                  AND l.""Latitude""  IS NOT NULL
                  AND l.""Longitude"" IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible: the pre-backfill Latitude/Longitude values
            // this overwrote are not recoverable.
        }
    }
}
