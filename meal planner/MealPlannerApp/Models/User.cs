using System.ComponentModel.DataAnnotations;

namespace MealPlannerApp.Models;

public class User : BaseEntity
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();
}
