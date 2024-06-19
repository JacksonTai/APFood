using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APFood.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerDeliveryTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryTasks_Customers_CustomerId",
                table: "DeliveryTasks");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryTasks_CustomerId",
                table: "DeliveryTasks");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DeliveryTasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "DeliveryTasks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryTasks_CustomerId",
                table: "DeliveryTasks",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryTasks_Customers_CustomerId",
                table: "DeliveryTasks",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}
