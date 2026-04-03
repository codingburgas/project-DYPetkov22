using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlannerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPlannerQuestionnairePreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BodyWeightKg",
                table: "Users",
                type: "REAL",
                nullable: false,
                defaultValue: 70.0);

            migrationBuilder.AddColumn<string>(
                name: "ExcludedFoods",
                table: "Users",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredMealsPerDay",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BodyWeightKg", "ExcludedFoods", "PreferredMealsPerDay" },
                values: new object[] { 70.0, null, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyWeightKg",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExcludedFoods",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredMealsPerDay",
                table: "Users");
        }
    }
}
