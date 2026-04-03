using MealPlannerApp.Data;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Services;

public class MealPlanTemplateService : IMealPlanTemplateService
{
    private readonly ApplicationDbContext _dbContext;

    public MealPlanTemplateService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<MealPlanTemplate>> GetVisibleTemplates(int? currentUserId, bool isAdmin)
    {
        return await GetVisibleTemplatesQuery(currentUserId, isAdmin)
            .Include(t => t.Owner)
            .Include(t => t.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .OrderByDescending(t => t.ApprovalStatus == ApprovalStatus.Approved)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<MealPlanTemplate?> GetTemplateById(int id, int? currentUserId, bool isAdmin)
    {
        return await GetVisibleTemplatesQuery(currentUserId, isAdmin)
            .Include(t => t.Owner)
            .Include(t => t.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<MealPlanTemplate> CreateFromWeek(int ownerId, DateTime weekStart, string name, string? description, bool submitForReview)
    {
        var normalizedWeekStart = Infrastructure.WeekDateHelper.GetWeekStart(weekStart);
        var weeklyPlan = await _dbContext.MealPlans
            .Where(mp => mp.UserId == ownerId && mp.Date >= normalizedWeekStart && mp.Date < normalizedWeekStart.AddDays(7))
            .Include(mp => mp.Meals)
            .ThenInclude(m => m.Recipe)
            .OrderBy(mp => mp.Date)
            .ToListAsync();

        var sourceMeals = weeklyPlan
            .SelectMany(plan => plan.Meals.Select(meal => new
            {
                DayOffset = (plan.Date.Date - normalizedWeekStart).Days,
                Meal = meal
            }))
            .Where(x => x.DayOffset is >= 0 and < 7)
            .OrderBy(x => x.DayOffset)
            .ThenBy(x => x.Meal.MealType)
            .ToList();

        if (sourceMeals.Count == 0)
        {
            throw new InvalidOperationException("Add meals to your week before creating a shareable plan template.");
        }

        var template = new MealPlanTemplate
        {
            OwnerId = ownerId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            WeekStart = normalizedWeekStart,
            CreatedAt = DateTime.UtcNow
        };
        ApplyReviewState(template, submitForReview ? ApprovalStatus.PendingReview : ApprovalStatus.Draft, null);

        foreach (var sourceMeal in sourceMeals)
        {
            template.Meals.Add(new MealPlanTemplateMeal
            {
                CreatedAt = DateTime.UtcNow,
                DayOffset = sourceMeal.DayOffset,
                MealType = sourceMeal.Meal.MealType,
                RecipeId = sourceMeal.Meal.RecipeId,
                PortionMultiplier = sourceMeal.Meal.PortionMultiplier
            });
        }

        _dbContext.MealPlanTemplates.Add(template);
        await _dbContext.SaveChangesAsync();
        return template;
    }

    public async Task<bool> SubmitForReview(int id, int ownerId, bool isAdmin)
    {
        var template = await _dbContext.MealPlanTemplates.FindAsync(id);
        if (template is null)
        {
            return false;
        }

        if (!CanManage(template, ownerId, isAdmin) || template.ApprovalStatus == ApprovalStatus.Approved)
        {
            return false;
        }

        ApplyReviewState(template, ApprovalStatus.PendingReview, null);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyCollection<MealPlanTemplate>> GetPendingTemplates()
    {
        return await _dbContext.MealPlanTemplates
            .Where(t => t.ApprovalStatus == ApprovalStatus.PendingReview)
            .Include(t => t.Owner)
            .Include(t => t.Meals)
            .ThenInclude(m => m.Recipe)
            .OrderBy(t => t.SubmittedAt)
            .ToListAsync();
    }

    public async Task<bool> ApproveTemplate(int id)
    {
        var template = await _dbContext.MealPlanTemplates
            .Include(t => t.Meals)
            .ThenInclude(m => m.Recipe)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (template is null)
        {
            return false;
        }

        var hasNonApprovedRecipe = template.Meals.Any(m => m.Recipe.ApprovalStatus != ApprovalStatus.Approved);
        if (hasNonApprovedRecipe)
        {
            return false;
        }

        ApplyReviewState(template, ApprovalStatus.Approved, null);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectTemplate(int id, string? reviewNotes)
    {
        var template = await _dbContext.MealPlanTemplates.FindAsync(id);
        if (template is null)
        {
            return false;
        }

        ApplyReviewState(template, ApprovalStatus.Rejected, reviewNotes);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApplyTemplateToWeek(int id, int currentUserId, DateTime weekStart, bool isAdmin)
    {
        var template = await GetTemplateById(id, currentUserId, isAdmin);
        if (template is null)
        {
            return false;
        }

        var blockedIngredientIds = await _dbContext.UserIngredientPreferences
            .Where(preference => preference.UserId == currentUserId)
            .Select(preference => preference.IngredientId)
            .ToListAsync();
        if (blockedIngredientIds.Count > 0 && template.Meals.Any(meal =>
                meal.Recipe.RecipeIngredients.Any(recipeIngredient => blockedIngredientIds.Contains(recipeIngredient.IngredientId))))
        {
            throw new InvalidOperationException("This template includes ingredients from your excluded or allergy list.");
        }

        var rangeStart = Infrastructure.WeekDateHelper.GetWeekStart(weekStart);
        var rangeEnd = rangeStart.AddDays(7);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var existingMealPlans = await _dbContext.MealPlans
            .Where(mp => mp.UserId == currentUserId && mp.Date >= rangeStart && mp.Date < rangeEnd)
            .Include(mp => mp.Meals)
            .OrderBy(mp => mp.Date)
            .ThenBy(mp => mp.Id)
            .ToListAsync();

        var duplicateMealPlans = existingMealPlans
            .GroupBy(mp => mp.Date.Date)
            .SelectMany(group => group.Skip(1))
            .ToList();
        if (duplicateMealPlans.Count > 0)
        {
            _dbContext.MealPlans.RemoveRange(duplicateMealPlans);
        }

        var mealPlansByDate = existingMealPlans
            .GroupBy(mp => mp.Date.Date)
            .ToDictionary(group => group.Key, group => group.First());

        var existingMeals = mealPlansByDate.Values.SelectMany(mp => mp.Meals).ToList();
        if (existingMeals.Count > 0)
        {
            _dbContext.Meals.RemoveRange(existingMeals);
        }

        for (var offset = 0; offset < 7; offset++)
        {
            var date = rangeStart.AddDays(offset).Date;
            if (mealPlansByDate.ContainsKey(date))
            {
                continue;
            }

            var mealPlan = new MealPlan
            {
                UserId = currentUserId,
                Date = date,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.MealPlans.Add(mealPlan);
            mealPlansByDate[date] = mealPlan;
        }

        await _dbContext.SaveChangesAsync();

        var mealsToAdd = template.Meals
            .OrderBy(m => m.DayOffset)
            .ThenBy(m => m.MealType)
            .Select(meal => new Meal
            {
                MealPlanId = mealPlansByDate[rangeStart.AddDays(meal.DayOffset).Date].Id,
                RecipeId = meal.RecipeId,
                MealType = meal.MealType,
                PortionMultiplier = meal.PortionMultiplier,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        _dbContext.Meals.AddRange(mealsToAdd);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private IQueryable<MealPlanTemplate> GetVisibleTemplatesQuery(int? currentUserId, bool isAdmin)
    {
        var query = _dbContext.MealPlanTemplates.AsQueryable();

        if (isAdmin)
        {
            return query;
        }

        if (!currentUserId.HasValue)
        {
            return query.Where(t => t.ApprovalStatus == ApprovalStatus.Approved);
        }

        return query.Where(t =>
            t.ApprovalStatus == ApprovalStatus.Approved ||
            t.OwnerId == currentUserId.Value);
    }

    private static bool CanManage(MealPlanTemplate template, int ownerId, bool isAdmin)
    {
        return isAdmin || template.OwnerId == ownerId;
    }

    private static void ApplyReviewState(MealPlanTemplate template, ApprovalStatus status, string? reviewNotes)
    {
        template.ApprovalStatus = status;
        template.ReviewNotes = string.IsNullOrWhiteSpace(reviewNotes) ? null : reviewNotes.Trim();

        if (status == ApprovalStatus.PendingReview)
        {
            template.SubmittedAt = DateTime.UtcNow;
            template.ReviewedAt = null;
            template.ReviewNotes = null;
            return;
        }

        if (status is ApprovalStatus.Approved or ApprovalStatus.Rejected)
        {
            template.ReviewedAt = DateTime.UtcNow;
            return;
        }

        template.SubmittedAt = null;
        template.ReviewedAt = null;
    }
}
