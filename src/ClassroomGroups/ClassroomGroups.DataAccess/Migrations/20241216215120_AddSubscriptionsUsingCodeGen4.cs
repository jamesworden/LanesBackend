using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionsUsingCodeGen4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 1,
                columns: new[] { "DisplayName", "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom", "SubscriptionType" },
                values: new object[] { "Free", new Guid("00000000-0000-0000-0000-000000000001"), 2, 3, 5, 30, "FREE" });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2,
                columns: new[] { "DisplayName", "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom", "SubscriptionType" },
                values: new object[] { "Basic", new Guid("00000000-0000-0000-0000-000000000002"), 5, 20, 20, 50, "BASIC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 1,
                columns: new[] { "DisplayName", "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom", "SubscriptionType" },
                values: new object[] { "Basic", new Guid("00000000-0000-0000-0000-000000000002"), 5, 20, 20, 50, "BASIC" });

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2,
                columns: new[] { "DisplayName", "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom", "SubscriptionType" },
                values: new object[] { "Free", new Guid("00000000-0000-0000-0000-000000000001"), 2, 3, 5, 30, "FREE" });
        }
    }
}
