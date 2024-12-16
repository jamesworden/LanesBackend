using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionsUsingCodeGen3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 1,
                columns: new[] { "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), 5, 20, 20, 50 });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2,
                columns: new[] { "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), 2, 3, 5, 30 });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 3,
                columns: new[] { "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom" },
                values: new object[] { 50, 50, 50, 100 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 1,
                columns: new[] { "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), 1, 5, 30, 5 });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2,
                columns: new[] { "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), 5, 1, 20, 20 });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 3,
                columns: new[] { "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom" },
                values: new object[] { 10, 5, 20, 500 });
        }
    }
}
