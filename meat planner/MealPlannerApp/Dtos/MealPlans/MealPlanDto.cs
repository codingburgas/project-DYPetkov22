using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Dtos.MealPlans;

public class MealPlanDto
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    public int MealsCount { get; set; }
    public int TotalCalories { get; set; }
    public IReadOnlyCollection<MealPlanMealDto> Meals { get; set; } = Array.Empty<MealPlanMealDto>();
    public IReadOnlyCollection<MostUsedIngredientSummaryDto> MostUsedIngredients { get; set; } = Array.Empty<MostUsedIngredientSummaryDto>();
}

public class MealPlanMealDto
{
    public string MealType { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public int Calories { get; set; }
}

public class MostUsedIngredientSummaryDto
{
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}
