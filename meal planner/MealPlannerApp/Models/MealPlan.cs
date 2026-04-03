using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Models;

public class MealPlan : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
}
