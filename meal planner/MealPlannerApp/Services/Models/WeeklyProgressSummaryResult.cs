namespace MealPlannerApp.Services.Models;

public class WeeklyProgressSummaryResult
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int MealsCount { get; set; }
    public int DaysWithMeals { get; set; }
    public NutritionSummaryResult TotalNutrition { get; set; } = new();
}
