namespace MealPlannerApp.Models;

public class Meal : BaseEntity
{
    public int MealPlanId { get; set; }
    public MealPlan MealPlan { get; set; } = null!;

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public MealType MealType { get; set; }
}
