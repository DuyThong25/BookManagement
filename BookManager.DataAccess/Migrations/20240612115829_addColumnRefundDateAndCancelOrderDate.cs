using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addColumnRefundDateAndCancelOrderDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefundDate",
                table: "OrderHeaders",
                newName: "RefundOrderDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelOrderDate",
                table: "OrderHeaders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelOrderDate",
                table: "OrderHeaders");

            migrationBuilder.RenameColumn(
                name: "RefundOrderDate",
                table: "OrderHeaders",
                newName: "RefundDate");
        }
    }
}
