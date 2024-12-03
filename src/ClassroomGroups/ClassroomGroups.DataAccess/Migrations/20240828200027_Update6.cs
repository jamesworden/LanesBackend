using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class Update6 : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<Guid>(
        name: "ClassroomId",
        table: "Students",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "GroupId",
        table: "StudentGroups",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "StudentId",
        table: "StudentGroups",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "FieldId",
        table: "StudentFields",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "StudentId",
        table: "StudentFields",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "ConfigurationId",
        table: "Groups",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "ClassroomId",
        table: "Fields",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "ClassroomId",
        table: "Configurations",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "ConfigurationId",
        table: "Columns",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "FieldId",
        table: "Columns",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );

      migrationBuilder.AddColumn<Guid>(
        name: "AccountId",
        table: "Classrooms",
        type: "TEXT",
        nullable: false,
        defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "ClassroomId", table: "Students");

      migrationBuilder.DropColumn(name: "GroupId", table: "StudentGroups");

      migrationBuilder.DropColumn(name: "StudentId", table: "StudentGroups");

      migrationBuilder.DropColumn(name: "FieldId", table: "StudentFields");

      migrationBuilder.DropColumn(name: "StudentId", table: "StudentFields");

      migrationBuilder.DropColumn(name: "ConfigurationId", table: "Groups");

      migrationBuilder.DropColumn(name: "ClassroomId", table: "Fields");

      migrationBuilder.DropColumn(name: "ClassroomId", table: "Configurations");

      migrationBuilder.DropColumn(name: "ConfigurationId", table: "Columns");

      migrationBuilder.DropColumn(name: "FieldId", table: "Columns");

      migrationBuilder.DropColumn(name: "AccountId", table: "Classrooms");
    }
  }
}
