using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Dtos.MealPlans;

public class GeneratePersonalizedMealPlanDto
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime WeekStart { get; set; } = DateTime.Today;

    [Display(Name = "Meals Per Day")]
    [Range(1, 3)]
    public int MealsPerDay { get; set; } = 3;

    [Display(Name = "Body Weight (kg)")]
    [Range(40, 180)]
    public double BodyWeightKg { get; set; } = 70;

    [Display(Name = "Protein Target (g/day)")]
    [Range(40, 300)]
    public double ProteinTargetGrams { get; set; } = 126;

    [Display(Name = "Carbs Target (g/day)")]
    [Range(20, 500)]
    public double CarbsTargetGrams { get; set; } = 273;

    [Display(Name = "Fat Target (g/day)")]
    [Range(20, 200)]
    public double FatTargetGrams { get; set; } = 56;

    [Display(Name = "Foods You Do Not Eat")]
    [StringLength(500)]
    public string? ExcludedFoods { get; set; }

    [Display(Name = "Excluded Ingredients")]
    public List<int> ExcludedIngredientIds { get; set; } = [];

    [Display(Name = "Allergy Ingredients")]
    public List<int> AllergyIngredientIds { get; set; } = [];
}
