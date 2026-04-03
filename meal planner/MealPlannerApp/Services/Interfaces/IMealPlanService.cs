using MealPlannerApp.Models;
using MealPlannerApp.Services.Models;

namespace MealPlannerApp.Services.Interfaces;

public interface IMealPlanService
{
    Task<IEnumerable<MealPlan>> GetAllMealPlans(int userId);
    Task<MealPlan?> GetMealPlanById(int id, int userId);
    Task<MealPlan> CreateMealPlan(int userId, MealPlan mealPlan);
    Task<bool> UpdateMealPlan(int userId, MealPlan mealPlan);
    Task<bool> DeleteMealPlan(int id, int userId);
    Task<Meal?> AddMealToPlan(int mealPlanId, int userId, bool isAdmin, Meal meal);
    Task<WeeklyMealPlanResult> GetWeeklyPlan(int userId, DateTime? weekStart = null);
    Task<IReadOnlyCollection<PresetMealPlanOptionResult>> GetPresetMealPlans();
    Task StartPresetMealPlan(StartPresetMealPlanRequest request);
    Task GeneratePersonalizedMealPlan(GeneratePersonalizedMealPlanRequest request);
    Task<PlannerPreferencesResult> GetPlannerPreferences(int userId);
    Task SavePlannerPreferences(SavePlannerPreferencesRequest request);
    Task<bool> SwapMeal(int mealId, int userId);
    Task<IReadOnlyCollection<WeeklyProgressSummaryResult>> GetWeeklyHistory(int userId, int weeks = 8);
}
