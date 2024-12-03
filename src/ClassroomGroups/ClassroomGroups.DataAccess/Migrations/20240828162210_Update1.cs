using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomGroups.DataAccess.Migrations
{
  /// <inheritdoc />
  public partial class Update1 : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(name: "AccountId", table: "Accounts", newName: "Id");

      migrationBuilder.RenameColumn(name: "AccountKey", table: "Accounts", newName: "Key");

      migrationBuilder.RenameIndex(
        name: "IX_Accounts_AccountId",
        table: "Accounts",
        newName: "IX_Accounts_Id"
      );

      migrationBuilder.CreateTable(
        name: "Classrooms",
        columns: table => new
        {
          Key = table
            .Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Id = table.Column<Guid>(type: "TEXT", nullable: false),
          Label = table.Column<string>(type: "TEXT", nullable: false),
          Description = table.Column<string>(type: "TEXT", nullable: true),
          AccountKey = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Classrooms", x => x.Key);
          table.ForeignKey(
            name: "FK_Classrooms_Accounts_AccountKey",
            column: x => x.AccountKey,
            principalTable: "Accounts",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "Configurations",
        columns: table => new
        {
          Key = table
            .Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Id = table.Column<Guid>(type: "TEXT", nullable: false),
          Label = table.Column<string>(type: "TEXT", nullable: false),
          Description = table.Column<string>(type: "TEXT", nullable: true),
          ClassroomDTOKey = table.Column<int>(type: "INTEGER", nullable: false),
          ClassroomKey = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Configurations", x => x.Key);
          table.ForeignKey(
            name: "FK_Configurations_Classrooms_ClassroomDTOKey",
            column: x => x.ClassroomDTOKey,
            principalTable: "Classrooms",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "Fields",
        columns: table => new
        {
          Key = table
            .Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Id = table.Column<Guid>(type: "TEXT", nullable: false),
          Type = table.Column<int>(type: "INTEGER", nullable: false),
          Label = table.Column<string>(type: "TEXT", nullable: false),
          ClassroomKey = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Fields", x => x.Key);
          table.ForeignKey(
            name: "FK_Fields_Classrooms_ClassroomKey",
            column: x => x.ClassroomKey,
            principalTable: "Classrooms",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "Students",
        columns: table => new
        {
          Key = table
            .Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Id = table.Column<Guid>(type: "TEXT", nullable: false),
          ClassroomKey = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Students", x => x.Key);
          table.ForeignKey(
            name: "FK_Students_Classrooms_ClassroomKey",
            column: x => x.ClassroomKey,
            principalTable: "Classrooms",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "Groups",
        columns: table => new
        {
          Key = table
            .Column<int>(type: "INTEGER", nullable: false)
            .Annotation("Sqlite:Autoincrement", true),
          Id = table.Column<Guid>(type: "TEXT", nullable: false),
          Label = table.Column<string>(type: "TEXT", nullable: false),
          Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
          ConfigurationKey = table.Column<int>(type: "INTEGER", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Groups", x => x.Key);
          table.ForeignKey(
            name: "FK_Groups_Configurations_ConfigurationKey",
            column: x => x.ConfigurationKey,
            principalTable: "Configurations",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "StudentFields",
        columns: table => new
        {
          StudentKey = table.Column<int>(type: "INTEGER", nullable: false),
          FieldKey = table.Column<int>(type: "INTEGER", nullable: false),
          Key = table.Column<int>(type: "INTEGER", nullable: false),
          Value = table.Column<string>(type: "TEXT", nullable: false),
          Id = table.Column<Guid>(type: "TEXT", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_StudentFields", x => new { x.StudentKey, x.FieldKey });
          table.ForeignKey(
            name: "FK_StudentFields_Fields_FieldKey",
            column: x => x.FieldKey,
            principalTable: "Fields",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_StudentFields_Students_StudentKey",
            column: x => x.StudentKey,
            principalTable: "Students",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "StudentGroups",
        columns: table => new
        {
          StudentKey = table.Column<int>(type: "INTEGER", nullable: false),
          GroupKey = table.Column<int>(type: "INTEGER", nullable: false),
          Key = table.Column<int>(type: "INTEGER", nullable: false),
          Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
          Id = table.Column<Guid>(type: "TEXT", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_StudentGroups", x => new { x.StudentKey, x.GroupKey });
          table.ForeignKey(
            name: "FK_StudentGroups_Groups_GroupKey",
            column: x => x.GroupKey,
            principalTable: "Groups",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_StudentGroups_Students_StudentKey",
            column: x => x.StudentKey,
            principalTable: "Students",
            principalColumn: "Key",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_Classrooms_AccountKey",
        table: "Classrooms",
        column: "AccountKey"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Classrooms_Id",
        table: "Classrooms",
        column: "Id",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Configurations_ClassroomDTOKey",
        table: "Configurations",
        column: "ClassroomDTOKey"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Configurations_Id",
        table: "Configurations",
        column: "Id",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Fields_ClassroomKey",
        table: "Fields",
        column: "ClassroomKey"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Fields_Id",
        table: "Fields",
        column: "Id",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Groups_ConfigurationKey",
        table: "Groups",
        column: "ConfigurationKey"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Groups_Id",
        table: "Groups",
        column: "Id",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_StudentFields_FieldKey",
        table: "StudentFields",
        column: "FieldKey"
      );

      migrationBuilder.CreateIndex(
        name: "IX_StudentFields_Id",
        table: "StudentFields",
        column: "Id",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_StudentGroups_GroupKey",
        table: "StudentGroups",
        column: "GroupKey"
      );

      migrationBuilder.CreateIndex(
        name: "IX_StudentGroups_Id",
        table: "StudentGroups",
        column: "Id",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Students_ClassroomKey",
        table: "Students",
        column: "ClassroomKey"
      );

      migrationBuilder.CreateIndex(
        name: "IX_Students_Id",
        table: "Students",
        column: "Id",
        unique: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "StudentFields");

      migrationBuilder.DropTable(name: "StudentGroups");

      migrationBuilder.DropTable(name: "Fields");

      migrationBuilder.DropTable(name: "Groups");

      migrationBuilder.DropTable(name: "Students");

      migrationBuilder.DropTable(name: "Configurations");

      migrationBuilder.DropTable(name: "Classrooms");

      migrationBuilder.RenameColumn(name: "Id", table: "Accounts", newName: "AccountId");

      migrationBuilder.RenameColumn(name: "Key", table: "Accounts", newName: "AccountKey");

      migrationBuilder.RenameIndex(
        name: "IX_Accounts_Id",
        table: "Accounts",
        newName: "IX_Accounts_AccountId"
      );
    }
  }
}
