using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Models;

public class Recipe : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(1, 300)]
    public int CookingTime { get; set; }

    public int Calories { get; set; }

    public bool IsVegetarian { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
}
