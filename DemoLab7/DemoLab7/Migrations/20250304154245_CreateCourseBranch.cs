using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoLab7.Migrations
{
    /// <inheritdoc />
    public partial class CreateCourseBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Course-Branch");

            migrationBuilder.RenameTable(
                name: "Teachers",
                schema: "public",
                newName: "Teachers",
                newSchema: "Course-Branch");

            migrationBuilder.RenameTable(
                name: "Students",
                schema: "public",
                newName: "Students",
                newSchema: "Course-Branch");

            migrationBuilder.RenameTable(
                name: "Courses",
                schema: "public",
                newName: "Courses",
                newSchema: "Course-Branch");

            migrationBuilder.RenameTable(
                name: "ClassesStudents",
                schema: "public",
                newName: "ClassesStudents",
                newSchema: "Course-Branch");

            migrationBuilder.RenameTable(
                name: "Classes",
                schema: "public",
                newName: "Classes",
                newSchema: "Course-Branch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Teachers",
                schema: "Course-Branch",
                newName: "Teachers",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Students",
                schema: "Course-Branch",
                newName: "Students",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Courses",
                schema: "Course-Branch",
                newName: "Courses",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ClassesStudents",
                schema: "Course-Branch",
                newName: "ClassesStudents",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Classes",
                schema: "Course-Branch",
                newName: "Classes",
                newSchema: "public");
        }
    }
}
