using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlannerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMacroTargetsAndIngredientPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CarbsTargetGrams",
                table: "Users",
                type: "REAL",
                nullable: false,
                defaultValue: 273.0);

            migrationBuilder.AddColumn<double>(
                name: "FatTargetGrams",
                table: "Users",
                type: "REAL",
                nullable: false,
                defaultValue: 56.0);

            migrationBuilder.AddColumn<double>(
                name: "ProteinTargetGrams",
                table: "Users",
                type: "REAL",
                nullable: false,
                defaultValue: 126.0);

            migrationBuilder.AddColumn<double>(
                name: "CarbsPer100g",
                table: "Ingredients",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FatPer100g",
                table: "Ingredients",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "ProteinPer100g",
                table: "Ingredients",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "UserIngredientPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    IngredientId = table.Column<int>(type: "INTEGER", nullable: false),
                    PreferenceType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIngredientPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIngredientPreferences_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserIngredientPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 0.0, 3.6000000000000001, 31.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 28.0, 0.29999999999999999, 2.7000000000000002 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 7.0, 0.40000000000000002, 2.7999999999999998 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 1.1000000000000001, 11.0, 13.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 3.6000000000000001, 0.40000000000000002, 2.8999999999999999 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 1.3, 33.0, 25.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 66.299999999999997, 6.9000000000000004, 16.899999999999999 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 3.6000000000000001, 0.40000000000000002, 10.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 22.800000000000001, 0.29999999999999999, 1.1000000000000001 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 0.0, 13.0, 20.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 20.100000000000001, 0.10000000000000001, 1.6000000000000001 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 27.399999999999999, 2.6000000000000001, 8.9000000000000004 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 3.8999999999999999, 0.20000000000000001, 0.90000000000000002 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 30.899999999999999, 0.90000000000000002, 5.7999999999999998 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 4.0999999999999996, 29.0, 38.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 6.0, 0.29999999999999999, 1.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 9.3000000000000007, 0.10000000000000001, 1.1000000000000001 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 33.100000000000001, 0.5, 6.4000000000000004 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 0.0, 100.0, 0.0 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 3.6000000000000001, 0.10000000000000001, 0.69999999999999996 });

            migrationBuilder.UpdateData(
                table: "Ingredients",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "CarbsPer100g", "FatPer100g", "ProteinPer100g" },
                values: new object[] { 8.5, 14.699999999999999, 2.0 });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CarbsTargetGrams", "FatTargetGrams", "ProteinTargetGrams" },
                values: new object[] { 273.0, 56.0, 126.0 });

            migrationBuilder.CreateIndex(
                name: "IX_UserIngredientPreferences_IngredientId",
                table: "UserIngredientPreferences",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIngredientPreferences_UserId_IngredientId_PreferenceType",
                table: "UserIngredientPreferences",
                columns: new[] { "UserId", "IngredientId", "PreferenceType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserIngredientPreferences");

            migrationBuilder.DropColumn(
                name: "CarbsTargetGrams",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FatTargetGrams",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProteinTargetGrams",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CarbsPer100g",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "FatPer100g",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "ProteinPer100g",
                table: "Ingredients");
        }
    }
}
