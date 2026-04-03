using System.ComponentModel.DataAnnotations;
using MealPlannerApp.Models;

namespace MealPlannerApp.Dtos.MealPlans;

public class AddMealDto
{
    [Range(1, int.MaxValue)]
    public int MealPlanId { get; set; }

    [Range(1, int.MaxValue)]
    public int RecipeId { get; set; }

    [Required]
    public MealType MealType { get; set; } = MealType.Breakfast;

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [DataType(DataType.Date)]
    public DateTime WeekStart { get; set; }
}
