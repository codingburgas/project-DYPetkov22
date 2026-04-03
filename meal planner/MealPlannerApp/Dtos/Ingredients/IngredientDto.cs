using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Dtos.Ingredients;

public class IngredientDto
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000)]
    public int CaloriesPer100g { get; set; }
}
