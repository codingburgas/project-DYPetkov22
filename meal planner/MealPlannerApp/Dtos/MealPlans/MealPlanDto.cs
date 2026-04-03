using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Dtos.MealPlans;

public class MealPlanDto
{
    public int Id { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    public int MealsCount { get; set; }
    public int TotalCalories { get; set; }
    public NutritionSummaryDto TotalNutrition { get; set; } = new();
    public IReadOnlyCollection<MealPlanMealDto> Meals { get; set; } = Array.Empty<MealPlanMealDto>();
    public IReadOnlyCollection<MostUsedIngredientSummaryDto> MostUsedIngredients { get; set; } = Array.Empty<MostUsedIngredientSummaryDto>();
}

public class MealPlanMealDto
{
    public int MealId { get; set; }
    public int RecipeId { get; set; }
    public string MealType { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double PortionMultiplier { get; set; }
    public NutritionSummaryDto Nutrition { get; set; } = new();
}

public class MostUsedIngredientSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public int TotalQuantityInGrams { get; set; }
}
