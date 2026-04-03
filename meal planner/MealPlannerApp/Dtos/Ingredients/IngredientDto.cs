using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Dtos.Ingredients;

public class IngredientDto
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000)]
    public int CaloriesPer100g { get; set; }

    [Display(Name = "Protein / 100g")]
    [Range(0, 100)]
    public double ProteinPer100g { get; set; }

    [Display(Name = "Carbs / 100g")]
    [Range(0, 100)]
    public double CarbsPer100g { get; set; }

    [Display(Name = "Fat / 100g")]
    [Range(0, 100)]
    public double FatPer100g { get; set; }
}
