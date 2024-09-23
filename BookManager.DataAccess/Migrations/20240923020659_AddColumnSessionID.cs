using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnSessionID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OrderHeaders");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "PaymentTransactions",
                newName: "SessionId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PaymentTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "PaymentTransactions");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "PaymentTransactions",
                newName: "TransactionId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "PaymentTypes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
