using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Models;

public class User : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    [Range(1, 3)]
    public int PreferredMealsPerDay { get; set; } = 3;

    [Range(40, 180)]
    public double BodyWeightKg { get; set; } = 70;

    [Range(40, 300)]
    public double ProteinTargetGrams { get; set; } = 126;

    [Range(20, 500)]
    public double CarbsTargetGrams { get; set; } = 273;

    [Range(20, 200)]
    public double FatTargetGrams { get; set; } = 56;

    [StringLength(500)]
    public string? ExcludedFoods { get; set; }

    public ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<MealPlanTemplate> MealPlanTemplates { get; set; } = new List<MealPlanTemplate>();
    public ICollection<UserIngredientPreference> IngredientPreferences { get; set; } = new List<UserIngredientPreference>();
}
