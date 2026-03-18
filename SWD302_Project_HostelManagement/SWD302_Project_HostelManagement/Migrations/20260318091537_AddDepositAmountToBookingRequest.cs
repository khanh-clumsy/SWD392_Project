using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWD302_Project_HostelManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositAmountToBookingRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount",
                table: "BookingRequest",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "BookingRequest");
        }
    }
}
