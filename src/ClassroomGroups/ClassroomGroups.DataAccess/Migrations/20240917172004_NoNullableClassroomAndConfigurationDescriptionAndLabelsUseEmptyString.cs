using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class NoNullableClassroomAndConfigurationDescriptionAndLabelsUseEmptyString
    : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "Description",
        table: "Configurations",
        type: "TEXT",
        nullable: false,
        defaultValue: "",
        oldClrType: typeof(string),
        oldType: "TEXT",
        oldNullable: true
      );

      migrationBuilder.AlterColumn<string>(
        name: "Description",
        table: "Classrooms",
        type: "TEXT",
        nullable: false,
        defaultValue: "",
        oldClrType: typeof(string),
        oldType: "TEXT",
        oldNullable: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AlterColumn<string>(
        name: "Description",
        table: "Configurations",
        type: "TEXT",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "TEXT"
      );

      migrationBuilder.AlterColumn<string>(
        name: "Description",
        table: "Classrooms",
        type: "TEXT",
        nullable: true,
        oldClrType: typeof(string),
        oldType: "TEXT"
      );
    }
  }
}
