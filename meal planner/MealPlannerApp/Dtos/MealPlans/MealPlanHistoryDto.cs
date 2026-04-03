namespace MealPlannerApp.Dtos.MealPlans;

public class MealPlanHistoryDto
{
    public IReadOnlyCollection<WeeklyProgressSummaryDto> Weeks { get; set; } = Array.Empty<WeeklyProgressSummaryDto>();
    public NutritionSummaryDto AverageDailyNutrition { get; set; } = new();
    public int AverageDailyCalories { get; set; }
}

public class WeeklyProgressSummaryDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int MealsCount { get; set; }
    public int DaysWithMeals { get; set; }
    public int TotalCalories { get; set; }
    public int AverageDailyCalories { get; set; }
    public NutritionSummaryDto TotalNutrition { get; set; } = new();
    public NutritionSummaryDto AverageDailyNutrition { get; set; } = new();
}
