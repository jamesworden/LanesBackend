using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class EnsureAccountGoogleNameIdentifierAreUnique : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateIndex(
        name: "IX_Accounts_GoogleNameIdentifier",
        table: "Accounts",
        column: "GoogleNameIdentifier",
        unique: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(name: "IX_Accounts_GoogleNameIdentifier", table: "Accounts");
    }
  }
}
