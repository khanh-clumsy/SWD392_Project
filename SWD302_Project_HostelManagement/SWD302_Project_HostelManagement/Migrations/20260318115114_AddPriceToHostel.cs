using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWD302_Project_HostelManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToHostel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "Hostel",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE h
                SET h.price = room_prices.min_price
                FROM Hostel AS h
                INNER JOIN (
                    SELECT hostel_id, MIN(price_per_month) AS min_price
                    FROM Room
                    GROUP BY hostel_id
                ) AS room_prices ON h.hostel_id = room_prices.hostel_id
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "price",
                table: "Hostel");
        }
    }
}
