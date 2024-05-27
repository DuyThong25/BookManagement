using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnOfUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressNumber",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "StreetName",
                table: "AspNetUsers",
                newName: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "AspNetUsers",
                newName: "StreetName");

            migrationBuilder.AddColumn<string>(
                name: "AddressNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
