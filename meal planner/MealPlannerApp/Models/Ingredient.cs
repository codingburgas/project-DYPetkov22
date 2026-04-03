using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Models;

public class Ingredient : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public int CaloriesPer100g { get; set; }
    public double ProteinPer100g { get; set; }
    public double CarbsPer100g { get; set; }
    public double FatPer100g { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<UserIngredientPreference> UserPreferences { get; set; } = new List<UserIngredientPreference>();
}
