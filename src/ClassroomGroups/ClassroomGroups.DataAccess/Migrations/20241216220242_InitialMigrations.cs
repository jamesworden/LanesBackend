using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClassroomGroups.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxClassrooms = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxStudentsPerClassroom = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxFieldsPerClassroom = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxConfigurationsPerClassroom = table.Column<int>(type: "INTEGER", nullable: false),
                    SubscriptionType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GoogleNameIdentifier = table.Column<string>(type: "TEXT", nullable: true),
                    PrimaryEmail = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionKey = table.Column<int>(type: "INTEGER", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Accounts_Subscriptions_SubscriptionKey",
                        column: x => x.SubscriptionKey,
                        principalTable: "Subscriptions",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AccountKey = table.Column<int>(type: "INTEGER", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Classrooms_Accounts_AccountKey",
                        column: x => x.AccountKey,
                        principalTable: "Accounts",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    ClassroomKey = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Fields_Classrooms_ClassroomKey",
                        column: x => x.ClassroomKey,
                        principalTable: "Classrooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClassroomKey = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Students_Classrooms_ClassroomKey",
                        column: x => x.ClassroomKey,
                        principalTable: "Classrooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentFields",
                columns: table => new
                {
                    StudentKey = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFields", x => new { x.StudentKey, x.FieldKey });
                    table.ForeignKey(
                        name: "FK_StudentFields_Fields_FieldKey",
                        column: x => x.FieldKey,
                        principalTable: "Fields",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentFields_Students_StudentKey",
                        column: x => x.StudentKey,
                        principalTable: "Students",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Columns",
                columns: table => new
                {
                    ConfigurationKey = table.Column<int>(type: "INTEGER", nullable: false),
                    FieldKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FieldId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sort = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Columns", x => new { x.FieldKey, x.ConfigurationKey });
                    table.ForeignKey(
                        name: "FK_Columns_Fields_FieldKey",
                        column: x => x.FieldKey,
                        principalTable: "Fields",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ClassroomKey = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DefaultGroupKey = table.Column<int>(type: "INTEGER", nullable: true),
                    DefaultGroupId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Configurations_Classrooms_ClassroomKey",
                        column: x => x.ClassroomKey,
                        principalTable: "Classrooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationKey = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Groups_Configurations_ConfigurationKey",
                        column: x => x.ConfigurationKey,
                        principalTable: "Configurations",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentGroups",
                columns: table => new
                {
                    StudentKey = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGroups", x => new { x.StudentKey, x.GroupKey });
                    table.ForeignKey(
                        name: "FK_StudentGroups_Groups_GroupKey",
                        column: x => x.GroupKey,
                        principalTable: "Groups",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentGroups_Students_StudentKey",
                        column: x => x.StudentKey,
                        principalTable: "Students",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Key", "DisplayName", "Id", "MaxClassrooms", "MaxConfigurationsPerClassroom", "MaxFieldsPerClassroom", "MaxStudentsPerClassroom", "SubscriptionType" },
                values: new object[,]
                {
                    { 1, "Free", new Guid("00000000-0000-0000-0000-000000000001"), 2, 3, 5, 30, "FREE" },
                    { 2, "Basic", new Guid("00000000-0000-0000-0000-000000000002"), 5, 20, 20, 50, "BASIC" },
                    { 3, "Pro", new Guid("00000000-0000-0000-0000-000000000003"), 50, 50, 50, 100, "PRO" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_GoogleNameIdentifier",
                table: "Accounts",
                column: "GoogleNameIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Id",
                table: "Accounts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SubscriptionKey",
                table: "Accounts",
                column: "SubscriptionKey");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_AccountKey",
                table: "Classrooms",
                column: "AccountKey");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_Id",
                table: "Classrooms",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Columns_ConfigurationKey",
                table: "Columns",
                column: "ConfigurationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_ClassroomKey",
                table: "Configurations",
                column: "ClassroomKey");

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_DefaultGroupKey",
                table: "Configurations",
                column: "DefaultGroupKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Configurations_Id",
                table: "Configurations",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fields_ClassroomKey",
                table: "Fields",
                column: "ClassroomKey");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_Id",
                table: "Fields",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ConfigurationKey",
                table: "Groups",
                column: "ConfigurationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Id",
                table: "Groups",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFields_FieldKey",
                table: "StudentFields",
                column: "FieldKey");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFields_Id",
                table: "StudentFields",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_GroupKey",
                table: "StudentGroups",
                column: "GroupKey");

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_Id",
                table: "StudentGroups",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassroomKey",
                table: "Students",
                column: "ClassroomKey");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Id",
                table: "Students",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Id",
                table: "Subscriptions",
                column: "Id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Columns_Configurations_ConfigurationKey",
                table: "Columns",
                column: "ConfigurationKey",
                principalTable: "Configurations",
                principalColumn: "Key",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Configurations_Groups_DefaultGroupKey",
                table: "Configurations",
                column: "DefaultGroupKey",
                principalTable: "Groups",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Subscriptions_SubscriptionKey",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Accounts_AccountKey",
                table: "Classrooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Configurations_ConfigurationKey",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "Columns");

            migrationBuilder.DropTable(
                name: "StudentFields");

            migrationBuilder.DropTable(
                name: "StudentGroups");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
