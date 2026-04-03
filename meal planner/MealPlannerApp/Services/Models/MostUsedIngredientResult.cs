namespace MealPlannerApp.Services.Models;

public class MostUsedIngredientResult
{
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int TotalQuantityInGrams { get; set; }
}
