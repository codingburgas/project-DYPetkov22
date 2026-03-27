using MealPlannerApp.Models;
using MealPlannerApp.Services.Models;

namespace MealPlannerApp.Services.Interfaces;

public interface IMealPlanService
{
    Task<IEnumerable<MealPlan>> GetAllMealPlans();
    Task<MealPlan?> GetMealPlanById(int id);
    Task<MealPlan> CreateMealPlan(MealPlan mealPlan);
    Task<bool> UpdateMealPlan(MealPlan mealPlan);
    Task<bool> DeleteMealPlan(int id);
    Task<Meal> AddMealToPlan(int mealPlanId, Meal meal);
    Task<WeeklyMealPlanResult> GetWeeklyPlan(int userId, DateTime? weekStart = null);
}
