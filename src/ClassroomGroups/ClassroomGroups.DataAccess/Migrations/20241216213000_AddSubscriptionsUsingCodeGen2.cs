using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionsUsingCodeGen2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 1,
                column: "Id",
                value: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2,
                column: "Id",
                value: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 3,
                column: "Id",
                value: new Guid("00000000-0000-0000-0000-000000000003"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 1,
                column: "Id",
                value: new Guid("bfc89c04-7dbe-4bf6-b3f9-9413f7d55996"));

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 2,
                column: "Id",
                value: new Guid("45eea37b-1ffb-49be-9fb6-540a11dcd3ff"));

            migrationBuilder.UpdateData(
                table: "Subscriptions",
                keyColumn: "Key",
                keyValue: 3,
                column: "Id",
                value: new Guid("06466377-2145-4658-84d7-8cbf7e0f0d48"));
        }
    }
}
