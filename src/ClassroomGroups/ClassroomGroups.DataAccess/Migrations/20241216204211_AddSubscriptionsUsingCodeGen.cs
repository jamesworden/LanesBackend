using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClassroomGroups.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionsUsingCodeGen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Key", "DisplayName", "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom", "SubscriptionType" },
                values: new object[,]
                {
                    { 1, "Basic", new Guid("bfc89c04-7dbe-4bf6-b3f9-9413f7d55996"), 1, 5, 30, 5, "BASIC" },
                    { 2, "Free", new Guid("45eea37b-1ffb-49be-9fb6-540a11dcd3ff"), 5, 1, 20, 20, "FREE" },
                    { 3, "Pro", new Guid("06466377-2145-4658-84d7-8cbf7e0f0d48"), 10, 5, 20, 500, "PRO" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 3);
        }
    }
}
