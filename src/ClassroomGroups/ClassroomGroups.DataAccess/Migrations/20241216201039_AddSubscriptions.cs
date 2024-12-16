using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionId",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionKey",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxClassrooms = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxStudentsPerClassroom = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxFieldsPerClassroom = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxConfigurationsPerClassroom = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SubscriptionKey",
                table: "Accounts",
                column: "SubscriptionKey");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Id",
                table: "Subscriptions",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Subscriptions_SubscriptionKey",
                table: "Accounts",
                column: "SubscriptionKey",
                principalTable: "Subscriptions",
                principalColumn: "Key",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Subscriptions_SubscriptionKey",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_SubscriptionKey",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SubscriptionKey",
                table: "Accounts");
        }
    }
}
