using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APFood.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryTaskStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RunnerDeliveryTasks",
                table: "RunnerDeliveryTasks");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DeliveryTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RunnerDeliveryTasks",
                table: "RunnerDeliveryTasks",
                columns: ["DeliveryTaskId", "RunnerId", "Status"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RunnerDeliveryTasks",
                table: "RunnerDeliveryTasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DeliveryTasks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RunnerDeliveryTasks",
                table: "RunnerDeliveryTasks",
                columns: ["DeliveryTaskId", "RunnerId"]);
        }
    }
}
