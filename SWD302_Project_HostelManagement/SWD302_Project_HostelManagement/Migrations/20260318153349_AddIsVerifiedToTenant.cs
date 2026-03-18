using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWD302_Project_HostelManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVerifiedToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Tenant",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Tenant");
        }
    }
}
