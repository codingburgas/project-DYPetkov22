using MealPlannerApp.Data;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using MealPlannerApp.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Services;

public class MealPlanService : IMealPlanService
{
    private readonly ApplicationDbContext _dbContext;

    public MealPlanService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<MealPlan>> GetAllMealPlans()
    {
        return await _dbContext.MealPlans
            .Include(mp => mp.User)
            .Include(mp => mp.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .OrderBy(mp => mp.Date)
            .ToListAsync();
    }

    public async Task<MealPlan?> GetMealPlanById(int id)
    {
        return await _dbContext.MealPlans
            .Include(mp => mp.User)
            .Include(mp => mp.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }

    public async Task<MealPlan> CreateMealPlan(MealPlan mealPlan)
    {
        mealPlan.CreatedAt = DateTime.UtcNow;
        _dbContext.MealPlans.Add(mealPlan);
        await _dbContext.SaveChangesAsync();
        return mealPlan;
    }

    public async Task<bool> UpdateMealPlan(MealPlan mealPlan)
    {
        var existingMealPlan = await _dbContext.MealPlans.FindAsync(mealPlan.Id);
        if (existingMealPlan is null)
        {
            return false;
        }

        existingMealPlan.UserId = mealPlan.UserId;
        existingMealPlan.Date = mealPlan.Date;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMealPlan(int id)
    {
        var mealPlan = await _dbContext.MealPlans.FindAsync(id);
        if (mealPlan is null)
        {
            return false;
        }

        _dbContext.MealPlans.Remove(mealPlan);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Meal> AddMealToPlan(int mealPlanId, Meal meal)
    {
        meal.MealPlanId = mealPlanId;
        meal.CreatedAt = DateTime.UtcNow;
        _dbContext.Meals.Add(meal);
        await _dbContext.SaveChangesAsync();
        return meal;
    }

    public async Task<WeeklyMealPlanResult> GetWeeklyPlan(int userId, DateTime? weekStart = null)
    {
        var startDate = weekStart?.Date ?? GetWeekStart(DateTime.UtcNow.Date);
        var endDate = startDate.AddDays(7);

        var mealPlans = await _dbContext.MealPlans
            .Where(mp => mp.UserId == userId && mp.Date >= startDate && mp.Date < endDate)
            .Include(mp => mp.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .ToListAsync();

        var dailyCalories = mealPlans
            .SelectMany(mp => mp.Meals.Select(m => new { Date = mp.Date.Date, Calories = m.Recipe.Calories }))
            .GroupBy(x => x.Date)
            .Select(g => new DailyCaloriesResult
            {
                Date = g.Key,
                TotalCalories = g.Sum(x => x.Calories)
            })
            .OrderBy(x => x.Date)
            .ToList();

        var weeklyCalories = dailyCalories.Sum(x => x.TotalCalories);
        var mostUsedIngredients = mealPlans
            .SelectMany(mp => mp.Meals)
            .SelectMany(m => m.Recipe.RecipeIngredients)
            .GroupBy(ri => ri.Ingredient.Name)
            .Select(g => new MostUsedIngredientResult
            {
                Name = g.Key,
                UsageCount = g.Count(),
                TotalQuantityInGrams = g.Sum(x => x.QuantityInGrams)
            })
            .OrderByDescending(x => x.UsageCount)
            .ThenByDescending(x => x.TotalQuantityInGrams)
            .Take(5)
            .ToList();

        return new WeeklyMealPlanResult
        {
            WeekStart = startDate,
            WeekEnd = endDate.AddDays(-1),
            WeeklyTotalCalories = weeklyCalories,
            DailyCalories = dailyCalories,
            MostUsedIngredients = mostUsedIngredients,
            MealPlans = mealPlans
        };
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}
