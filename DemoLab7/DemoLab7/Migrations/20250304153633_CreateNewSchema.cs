using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoLab7.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Teachers",
                newName: "Teachers",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Students",
                newName: "Students",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "Courses",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ClassesStudents",
                newName: "ClassesStudents",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Classes",
                newName: "Classes",
                newSchema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Teachers",
                schema: "public",
                newName: "Teachers");

            migrationBuilder.RenameTable(
                name: "Students",
                schema: "public",
                newName: "Students");

            migrationBuilder.RenameTable(
                name: "Courses",
                schema: "public",
                newName: "Courses");

            migrationBuilder.RenameTable(
                name: "ClassesStudents",
                schema: "public",
                newName: "ClassesStudents");

            migrationBuilder.RenameTable(
                name: "Classes",
                schema: "public",
                newName: "Classes");
        }
    }
}
