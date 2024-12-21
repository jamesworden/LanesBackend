using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReduceStudentCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2,
                column: "MaxStudentsPerClassroom",
                value: 40);

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 3,
                column: "MaxStudentsPerClassroom",
                value: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2,
                column: "MaxStudentsPerClassroom",
                value: 50);

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 3,
                column: "MaxStudentsPerClassroom",
                value: 100);
        }
    }
}
