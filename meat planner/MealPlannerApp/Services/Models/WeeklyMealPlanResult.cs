using MealPlannerApp.Models;

namespace MealPlannerApp.Services.Models;

public class WeeklyMealPlanResult
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int WeeklyTotalCalories { get; set; }
    public IReadOnlyCollection<DailyCaloriesResult> DailyCalories { get; set; } = Array.Empty<DailyCaloriesResult>();
    public IReadOnlyCollection<MostUsedIngredientResult> MostUsedIngredients { get; set; } = Array.Empty<MostUsedIngredientResult>();
    public IReadOnlyCollection<MealPlan> MealPlans { get; set; } = Array.Empty<MealPlan>();
}
