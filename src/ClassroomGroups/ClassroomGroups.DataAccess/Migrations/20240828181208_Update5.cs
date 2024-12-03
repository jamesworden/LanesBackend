using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class Update5 : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_Columns_Configurations_ConfigurationDTOKey",
        table: "Columns"
      );

      migrationBuilder.DropForeignKey(name: "FK_Columns_Fields_FieldDTOKey", table: "Columns");

      migrationBuilder.DropPrimaryKey(name: "PK_Columns", table: "Columns");

      migrationBuilder.DropIndex(name: "IX_Columns_ConfigurationDTOKey", table: "Columns");

      migrationBuilder.DropIndex(name: "IX_Columns_FieldDTOKey", table: "Columns");

      migrationBuilder.DropColumn(name: "ConfigurationDTOKey", table: "Columns");

      migrationBuilder.DropColumn(name: "FieldDTOKey", table: "Columns");

      migrationBuilder
        .AlterColumn<int>(
          name: "Key",
          table: "Columns",
          type: "INTEGER",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "INTEGER"
        )
        .OldAnnotation("Sqlite:Autoincrement", true);

      migrationBuilder.AddPrimaryKey(
        name: "PK_Columns",
        table: "Columns",
        columns: new[] { "FieldKey", "ConfigurationKey" }
      );

      migrationBuilder.CreateIndex(
        name: "IX_Columns_ConfigurationKey",
        table: "Columns",
        column: "ConfigurationKey"
      );

      migrationBuilder.AddForeignKey(
        name: "FK_Columns_Configurations_ConfigurationKey",
        table: "Columns",
        column: "ConfigurationKey",
        principalTable: "Configurations",
        principalColumn: "Key",
        onDelete: ReferentialAction.Cascade
      );

      migrationBuilder.AddForeignKey(
        name: "FK_Columns_Fields_FieldKey",
        table: "Columns",
        column: "FieldKey",
        principalTable: "Fields",
        principalColumn: "Key",
        onDelete: ReferentialAction.Cascade
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
        name: "FK_Columns_Configurations_ConfigurationKey",
        table: "Columns"
      );

      migrationBuilder.DropForeignKey(name: "FK_Columns_Fields_FieldKey", table: "Columns");

      migrationBuilder.DropPrimaryKey(name: "PK_Columns", table: "Columns");

      migrationBuilder.DropIndex(name: "IX_Columns_ConfigurationKey", table: "Columns");

      migrationBuilder
        .AlterColumn<int>(
          name: "Key",
          table: "Columns",
          type: "INTEGER",
          nullable: false,
          oldClrType: typeof(int),
          oldType: "INTEGER"
        )
        .Annotation("Sqlite:Autoincrement", true);

      migrationBuilder.AddColumn<int>(
        name: "ConfigurationDTOKey",
        table: "Columns",
        type: "INTEGER",
        nullable: false,
        defaultValue: 0
      );

      migrationBuilder.AddColumn<int>(
        name: "FieldDTOKey",
        table: "Columns",
        type: "INTEGER",
        nullable: false,
        defaultValue: 0
      );

      migrationBuilder.AddPrimaryKey(name: "PK_Columns", table: "Columns", column: "Key");

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

      migrationBuilder.AddForeignKey(
        name: "FK_Columns_Configurations_ConfigurationDTOKey",
        table: "Columns",
        column: "ConfigurationDTOKey",
        principalTable: "Configurations",
        principalColumn: "Key",
        onDelete: ReferentialAction.Cascade
      );

      migrationBuilder.AddForeignKey(
        name: "FK_Columns_Fields_FieldDTOKey",
        table: "Columns",
        column: "FieldDTOKey",
        principalTable: "Fields",
        principalColumn: "Key",
        onDelete: ReferentialAction.Cascade
      );
    }
  }
}
