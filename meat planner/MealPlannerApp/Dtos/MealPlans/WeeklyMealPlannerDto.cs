namespace MealPlannerApp.Dtos.MealPlans;

public class WeeklyMealPlannerDto
{
    public int UserId { get; set; }
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int WeeklyTotalCalories { get; set; }
    public IReadOnlyCollection<WeeklyDayDto> Days { get; set; } = Array.Empty<WeeklyDayDto>();
    public IReadOnlyCollection<MostUsedIngredientDto> MostUsedIngredients { get; set; } = Array.Empty<MostUsedIngredientDto>();
}

public class WeeklyDayDto
{
    public DateTime Date { get; set; }
    public int TotalCalories { get; set; }
    public IReadOnlyCollection<WeeklyMealItemDto> Meals { get; set; } = Array.Empty<WeeklyMealItemDto>();
}

public class WeeklyMealItemDto
{
    public string MealType { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public int Calories { get; set; }
}

public class MostUsedIngredientDto
{
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int TotalQuantityInGrams { get; set; }
}
