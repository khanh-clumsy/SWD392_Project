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
