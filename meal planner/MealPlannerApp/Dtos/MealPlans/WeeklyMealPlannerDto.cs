namespace MealPlannerApp.Dtos.MealPlans;

public class WeeklyMealPlannerDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public DateTime SelectedDate { get; set; }
    public int WeeklyTotalCalories { get; set; }
    public NutritionSummaryDto WeeklyNutrition { get; set; } = new();
    public NutritionSummaryDto WeeklyNutritionTarget { get; set; } = new();
    public NutritionSummaryDto DailyNutritionTarget { get; set; } = new();
    public IReadOnlyCollection<WeeklyDayDto> Days { get; set; } = Array.Empty<WeeklyDayDto>();
    public IReadOnlyCollection<MostUsedIngredientDto> MostUsedIngredients { get; set; } = Array.Empty<MostUsedIngredientDto>();
    public IReadOnlyCollection<PlannerIngredientOptionDto> AvailableIngredients { get; set; } = Array.Empty<PlannerIngredientOptionDto>();
    public IReadOnlyCollection<PresetMealPlanOptionDto> PresetPlans { get; set; } = Array.Empty<PresetMealPlanOptionDto>();
    public GeneratePersonalizedMealPlanDto GeneratePlan { get; set; } = new();
    public StartPresetMealPlanDto StartPresetPlan { get; set; } = new();
}

public class WeeklyDayDto
{
    public DateTime Date { get; set; }
    public int TotalCalories { get; set; }
    public NutritionSummaryDto Nutrition { get; set; } = new();
    public IReadOnlyCollection<WeeklyMealItemDto> Meals { get; set; } = Array.Empty<WeeklyMealItemDto>();
}

public class WeeklyMealItemDto
{
    public int MealId { get; set; }
    public int RecipeId { get; set; }
    public string MealType { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double PortionMultiplier { get; set; }
    public NutritionSummaryDto Nutrition { get; set; } = new();
}

public class MostUsedIngredientDto
{
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int TotalQuantityInGrams { get; set; }
}

public class PlannerIngredientOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
