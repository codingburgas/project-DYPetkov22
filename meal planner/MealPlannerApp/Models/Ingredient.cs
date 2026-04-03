using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Models;

public class Ingredient : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public int CaloriesPer100g { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
