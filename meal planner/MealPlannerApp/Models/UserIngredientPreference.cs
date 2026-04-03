namespace MealPlannerApp.Models;

public class UserIngredientPreference : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    public UserIngredientPreferenceType PreferenceType { get; set; }
}
