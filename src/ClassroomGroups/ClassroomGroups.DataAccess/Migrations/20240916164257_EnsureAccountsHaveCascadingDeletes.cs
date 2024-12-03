using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class EnsureAccountsHaveCascadingDeletes : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_Classrooms_Accounts_AccountKey",
        table: "Classrooms"
      );

      migrationBuilder.AddForeignKey(
        name: "FK_Classrooms_Accounts_AccountKey",
        table: "Classrooms",
        column: "AccountKey",
        principalTable: "Accounts",
        principalColumn: "Key",
        onDelete: ReferentialAction.Cascade
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_Classrooms_Accounts_AccountKey",
        table: "Classrooms"
      );

      migrationBuilder.AddForeignKey(
        name: "FK_Classrooms_Accounts_AccountKey",
        table: "Classrooms",
        column: "AccountKey",
        principalTable: "Accounts",
        principalColumn: "Key",
        onDelete: ReferentialAction.Restrict
      );
    }
  }
}
