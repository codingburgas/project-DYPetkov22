using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Models;

public class RecipeIngredient : BaseEntity
{
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int QuantityInGrams { get; set; }
}
