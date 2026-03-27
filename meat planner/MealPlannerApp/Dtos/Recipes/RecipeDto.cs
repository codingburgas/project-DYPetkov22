using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Dtos.Recipes;

public class RecipeDto
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(1, 300)]
    public int CookingTime { get; set; }

    [Range(0, 10000)]
    public int Calories { get; set; }

    public bool IsVegetarian { get; set; }
}
