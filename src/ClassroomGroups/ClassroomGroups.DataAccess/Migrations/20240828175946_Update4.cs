using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class Update4 : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "Columns",
        columns: table => new
        {
          Key = table
            .Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Id = table.Column<Guid>(type: "TEXT", nullable: false),
          ConfigurationDTOKey = table.Column<int>(type: "INTEGER", nullable: false),
          ConfigurationKey = table.Column<int>(type: "INTEGER", nullable: false),
          FieldDTOKey = table.Column<int>(type: "INTEGER", nullable: false),
          FieldKey = table.Column<int>(type: "INTEGER", nullable: false),
          Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
          Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
          Sort = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Columns", x => x.Key);
          table.ForeignKey(
            name: "FK_Columns_Configurations_ConfigurationDTOKey",
            column: x => x.ConfigurationDTOKey,
            principalTable: "Configurations",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_Columns_Fields_FieldDTOKey",
            column: x => x.FieldDTOKey,
            principalTable: "Fields",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_Columns_ConfigurationDTOKey",
        table: "Columns",
        column: "ConfigurationDTOKey"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Columns_FieldDTOKey",
        table: "Columns",
        column: "FieldDTOKey"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "Columns");
    }
  }
}
