namespace MealPlannerApp.Services.Models;

public class GeneratePersonalizedMealPlanRequest
{
    public int UserId { get; set; }
    public DateTime WeekStart { get; set; }
    public int MealsPerDay { get; set; }
    public double BodyWeightKg { get; set; }
    public double ProteinTargetGrams { get; set; }
    public double CarbsTargetGrams { get; set; }
    public double FatTargetGrams { get; set; }
    public IReadOnlyCollection<string> ExcludedFoods { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<int> ExcludedIngredientIds { get; set; } = Array.Empty<int>();
    public IReadOnlyCollection<int> AllergyIngredientIds { get; set; } = Array.Empty<int>();
}
