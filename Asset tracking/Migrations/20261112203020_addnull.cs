using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asset_tracking.Migrations
{
    /// <inheritdoc />
    public partial class addnull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "warrantyExpirationDate",
                table: "Assets",
                newName: "WarrantyExpirationDate");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeUserName",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WarrantyExpirationDate",
                table: "Assets",
                newName: "warrantyExpirationDate");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeUserName",
                table: "Assets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
