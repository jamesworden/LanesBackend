using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class Update3 : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_Configurations_Classrooms_ClassroomDTOKey",
        table: "Configurations"
      );

      migrationBuilder.DropIndex(
        name: "IX_Configurations_ClassroomDTOKey",
        table: "Configurations"
      );

      migrationBuilder.DropColumn(name: "ClassroomDTOKey", table: "Configurations");

      migrationBuilder.CreateIndex(
        name: "IX_Configurations_ClassroomKey",
        table: "Configurations",
        column: "ClassroomKey"
      );

      migrationBuilder.AddForeignKey(
        name: "FK_Configurations_Classrooms_ClassroomKey",
        table: "Configurations",
        column: "ClassroomKey",
        principalTable: "Classrooms",
        principalColumn: "Key",
        onDelete: ReferentialAction.Cascade
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_Configurations_Classrooms_ClassroomKey",
        table: "Configurations"
      );

      migrationBuilder.DropIndex(name: "IX_Configurations_ClassroomKey", table: "Configurations");

      migrationBuilder.AddColumn<int>(
        name: "ClassroomDTOKey",
        table: "Configurations",
        type: "INTEGER",
        nullable: false,
        defaultValue: 0
      );

      migrationBuilder.CreateIndex(
        name: "IX_Configurations_ClassroomDTOKey",
        table: "Configurations",
        column: "ClassroomDTOKey"
      );

      migrationBuilder.AddForeignKey(
        name: "FK_Configurations_Classrooms_ClassroomDTOKey",
        table: "Configurations",
        column: "ClassroomDTOKey",
        principalTable: "Classrooms",
        principalColumn: "Key",
        onDelete: ReferentialAction.Cascade
      );
    }
  }
}
