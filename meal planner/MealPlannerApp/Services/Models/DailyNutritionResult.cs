namespace MealPlannerApp.Services.Models;

public class DailyNutritionResult
{
    public DateTime Date { get; set; }
    public NutritionSummaryResult Nutrition { get; set; } = new();
}
