using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTableMultiplePaymentgate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentTransactionId",
                table: "OrderHeaders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Alias", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Stripe payment gateway", true, "Stripe" },
                    { 2, "MoMo payment gateway", true, "MoMo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderHeaders_PaymentTransactionId",
                table: "OrderHeaders",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentTypeId",
                table: "PaymentTransactions",
                column: "PaymentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHeaders_PaymentTransactions_PaymentTransactionId",
                table: "OrderHeaders",
                column: "PaymentTransactionId",
                principalTable: "PaymentTransactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHeaders_PaymentTransactions_PaymentTransactionId",
                table: "OrderHeaders");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropIndex(
                name: "IX_OrderHeaders_PaymentTransactionId",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "PaymentTransactionId",
                table: "OrderHeaders");
        }
    }
}
