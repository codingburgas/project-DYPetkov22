using MealPlannerApp.Models;

namespace MealPlannerApp.Dtos.MealPlanTemplates;

public class MealPlanTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public string OwnerUserName { get; set; } = string.Empty;
    public ApprovalStatus ApprovalStatus { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int WeeklyTotalCalories { get; set; }
    public IReadOnlyCollection<MealPlanTemplateDayDto> Days { get; set; } = Array.Empty<MealPlanTemplateDayDto>();
}

public class MealPlanTemplateDayDto
{
    public DateTime Date { get; set; }
    public int TotalCalories { get; set; }
    public IReadOnlyCollection<MealPlanTemplateMealDto> Meals { get; set; } = Array.Empty<MealPlanTemplateMealDto>();
}

public class MealPlanTemplateMealDto
{
    public int RecipeId { get; set; }
    public string MealType { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public int Calories { get; set; }
    public double PortionMultiplier { get; set; }
}
