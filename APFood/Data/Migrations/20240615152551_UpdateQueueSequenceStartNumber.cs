using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APFood.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQueueSequenceStartNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RestartSequence(
                name: "QueueNumberSequence",
                schema: "dbo",
                startValue: 1L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RestartSequence(
                name: "QueueNumberSequence",
                schema: "dbo",
                startValue: 1000L);
        }
    }
}
