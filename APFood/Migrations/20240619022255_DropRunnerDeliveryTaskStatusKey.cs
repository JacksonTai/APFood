using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APFood.Migrations
{
    /// <inheritdoc />
    public partial class DropRunnerDeliveryTaskStatusKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RunnerDeliveryTasks",
                table: "RunnerDeliveryTasks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RunnerDeliveryTasks",
                table: "RunnerDeliveryTasks",
                columns: new[] { "DeliveryTaskId", "RunnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RunnerDeliveryTasks",
                table: "RunnerDeliveryTasks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RunnerDeliveryTasks",
                table: "RunnerDeliveryTasks",
                columns: new[] { "DeliveryTaskId", "RunnerId", "Status" });
        }
    }
}
