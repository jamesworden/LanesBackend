using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionsEnumPropertyConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubscriptionType",
                table: "Subscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionType",
                table: "Subscriptions");
        }
    }
}
