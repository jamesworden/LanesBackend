using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class InitialCreate : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "Accounts",
        columns: table => new
        {
          AccountKey = table
            .Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          GoogleNameIdentifier = table.Column<string>(type: "TEXT", nullable: true),
          PrimaryEmail = table.Column<string>(type: "TEXT", nullable: false),
          AccountId = table.Column<Guid>(type: "TEXT", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Accounts", x => x.AccountKey);
        }
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "Accounts");
    }
  }
}
