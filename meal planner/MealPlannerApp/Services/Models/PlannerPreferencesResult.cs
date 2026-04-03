namespace MealPlannerApp.Services.Models;

public class PlannerPreferencesResult
{
    public int MealsPerDay { get; set; }
    public double BodyWeightKg { get; set; }
    public double ProteinTargetGrams { get; set; }
    public double CarbsTargetGrams { get; set; }
    public double FatTargetGrams { get; set; }
    public string? ExcludedFoods { get; set; }
    public IReadOnlyCollection<int> ExcludedIngredientIds { get; set; } = Array.Empty<int>();
    public IReadOnlyCollection<int> AllergyIngredientIds { get; set; } = Array.Empty<int>();
}
