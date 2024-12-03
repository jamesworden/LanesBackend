using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class AddDefaultGroup : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<Guid>(
        name: "DefaultGroupId",
        table: "Configurations",
        type: "TEXT",
        nullable: true
      );

      migrationBuilder.AddColumn<int>(
        name: "DefaultGroupKey",
        table: "Configurations",
        type: "INTEGER",
        nullable: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Configurations_DefaultGroupKey",
        table: "Configurations",
        column: "DefaultGroupKey",
        unique: true
      );

      migrationBuilder.AddForeignKey(
        name: "FK_Configurations_Groups_DefaultGroupKey",
        table: "Configurations",
        column: "DefaultGroupKey",
        principalTable: "Groups",
        principalColumn: "Key",
        onDelete: ReferentialAction.Restrict
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_Configurations_Groups_DefaultGroupKey",
        table: "Configurations"
      );

      migrationBuilder.DropIndex(
        name: "IX_Configurations_DefaultGroupKey",
        table: "Configurations"
      );

      migrationBuilder.DropColumn(name: "DefaultGroupId", table: "Configurations");

      migrationBuilder.DropColumn(name: "DefaultGroupKey", table: "Configurations");
    }
  }
}
