using MealPlannerApp.Data;
using MealPlannerApp.Infrastructure;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using MealPlannerApp.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Services;

public class MealPlanService : IMealPlanService
{
    private static readonly IReadOnlyCollection<string> BreakfastKeywords =
    [
        "breakfast",
        "oat",
        "yogurt",
        "banana",
        "omelette",
        "egg",
        "toast"
    ];

    private static readonly IReadOnlyCollection<string> HeartyMealKeywords =
    [
        "chicken",
        "salmon",
        "pasta",
        "rice",
        "tray bake",
        "skillet",
        "bowl",
        "roast"
    ];

    private static readonly IReadOnlyCollection<PresetMealPlanDefinition> PresetMealPlanDefinitions =
    [
        new(
            "balanced-starter",
            "Balanced Starter Week",
            "Simple mixed meals with repeated favorites so a beginner can start a full week without planning from scratch.",
            [
                new(0, MealType.Breakfast, 4),
                new(0, MealType.Lunch, 1),
                new(0, MealType.Dinner, 5),
                new(1, MealType.Breakfast, 3),
                new(1, MealType.Lunch, 7),
                new(1, MealType.Dinner, 8),
                new(2, MealType.Breakfast, 4),
                new(2, MealType.Lunch, 1),
                new(2, MealType.Dinner, 6),
                new(3, MealType.Breakfast, 3),
                new(3, MealType.Lunch, 5),
                new(3, MealType.Dinner, 8),
                new(4, MealType.Breakfast, 4),
                new(4, MealType.Lunch, 7),
                new(4, MealType.Dinner, 1),
                new(5, MealType.Breakfast, 3),
                new(5, MealType.Lunch, 6),
                new(5, MealType.Dinner, 5),
                new(6, MealType.Breakfast, 4),
                new(6, MealType.Lunch, 1),
                new(6, MealType.Dinner, 7)
            ]),
        new(
            "high-protein-starter",
            "High-Protein Starter Week",
            "Built around eggs, chicken, and salmon for people who want an easy higher-protein starting template.",
            [
                new(0, MealType.Breakfast, 3),
                new(0, MealType.Lunch, 1),
                new(0, MealType.Dinner, 5),
                new(1, MealType.Breakfast, 4),
                new(1, MealType.Lunch, 7),
                new(1, MealType.Dinner, 1),
                new(2, MealType.Breakfast, 3),
                new(2, MealType.Lunch, 1),
                new(2, MealType.Dinner, 5),
                new(3, MealType.Breakfast, 4),
                new(3, MealType.Lunch, 7),
                new(3, MealType.Dinner, 5),
                new(4, MealType.Breakfast, 3),
                new(4, MealType.Lunch, 1),
                new(4, MealType.Dinner, 7),
                new(5, MealType.Breakfast, 4),
                new(5, MealType.Lunch, 1),
                new(5, MealType.Dinner, 5),
                new(6, MealType.Breakfast, 3),
                new(6, MealType.Lunch, 7),
                new(6, MealType.Dinner, 5)
            ]),
        new(
            "vegetarian-starter",
            "Vegetarian Starter Week",
            "An easier vegetarian week with a small recipe rotation and straightforward prep for each day.",
            [
                new(0, MealType.Breakfast, 4),
                new(0, MealType.Lunch, 2),
                new(0, MealType.Dinner, 6),
                new(1, MealType.Breakfast, 3),
                new(1, MealType.Lunch, 8),
                new(1, MealType.Dinner, 2),
                new(2, MealType.Breakfast, 4),
                new(2, MealType.Lunch, 6),
                new(2, MealType.Dinner, 3),
                new(3, MealType.Breakfast, 3),
                new(3, MealType.Lunch, 2),
                new(3, MealType.Dinner, 8),
                new(4, MealType.Breakfast, 4),
                new(4, MealType.Lunch, 6),
                new(4, MealType.Dinner, 2),
                new(5, MealType.Breakfast, 3),
                new(5, MealType.Lunch, 8),
                new(5, MealType.Dinner, 6),
                new(6, MealType.Breakfast, 4),
                new(6, MealType.Lunch, 2),
                new(6, MealType.Dinner, 6)
            ])
    ];

    private readonly ApplicationDbContext _dbContext;

    public MealPlanService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlannerPreferencesResult> GetPlannerPreferences(int userId)
    {
        var preferences = await GetStoredPlannerPreferencesSnapshot(userId);
        return new PlannerPreferencesResult
        {
            MealsPerDay = preferences.MealsPerDay,
            BodyWeightKg = preferences.BodyWeightKg,
            ProteinTargetGrams = preferences.DailyNutritionTarget.ProteinGrams,
            CarbsTargetGrams = preferences.DailyNutritionTarget.CarbsGrams,
            FatTargetGrams = preferences.DailyNutritionTarget.FatGrams,
            ExcludedFoods = preferences.ExcludedFoodTerms.Count == 0
                ? null
                : string.Join(", ", preferences.ExcludedFoodTerms),
            ExcludedIngredientIds = preferences.ExcludedIngredientIds.ToArray(),
            AllergyIngredientIds = preferences.AllergyIngredientIds.ToArray()
        };
    }

    public async Task SavePlannerPreferences(SavePlannerPreferencesRequest request)
    {
        var user = await _dbContext.Users
            .Include(currentUser => currentUser.IngredientPreferences)
            .FirstOrDefaultAsync(currentUser => currentUser.Id == request.UserId);
        if (user is null)
        {
            throw new InvalidOperationException("The selected user does not exist.");
        }

        var validIngredientIds = await _dbContext.Ingredients
            .Where(ingredient =>
                request.ExcludedIngredientIds.Contains(ingredient.Id) ||
                request.AllergyIngredientIds.Contains(ingredient.Id))
            .Select(ingredient => ingredient.Id)
            .ToListAsync();

        var preferences = CreatePlannerPreferencesSnapshot(
            request.MealsPerDay,
            request.BodyWeightKg,
            request.ProteinTargetGrams,
            request.CarbsTargetGrams,
            request.FatTargetGrams,
            SplitExcludedFoods(request.ExcludedFoods),
            request.ExcludedIngredientIds.Intersect(validIngredientIds),
            request.AllergyIngredientIds.Intersect(validIngredientIds));

        user.PreferredMealsPerDay = preferences.MealsPerDay;
        user.BodyWeightKg = preferences.BodyWeightKg;
        user.ProteinTargetGrams = preferences.DailyNutritionTarget.ProteinGrams;
        user.CarbsTargetGrams = preferences.DailyNutritionTarget.CarbsGrams;
        user.FatTargetGrams = preferences.DailyNutritionTarget.FatGrams;
        user.ExcludedFoods = preferences.ExcludedFoodTerms.Count == 0
            ? null
            : string.Join(", ", preferences.ExcludedFoodTerms);

        if (user.IngredientPreferences.Count > 0)
        {
            _dbContext.UserIngredientPreferences.RemoveRange(user.IngredientPreferences);
        }

        var ingredientPreferences = preferences.ExcludedIngredientIds
            .Select(ingredientId => new UserIngredientPreference
            {
                UserId = user.Id,
                IngredientId = ingredientId,
                PreferenceType = UserIngredientPreferenceType.Excluded,
                CreatedAt = DateTime.UtcNow
            })
            .Concat(preferences.AllergyIngredientIds.Select(ingredientId => new UserIngredientPreference
            {
                UserId = user.Id,
                IngredientId = ingredientId,
                PreferenceType = UserIngredientPreferenceType.Allergy,
                CreatedAt = DateTime.UtcNow
            }))
            .ToList();

        if (ingredientPreferences.Count > 0)
        {
            _dbContext.UserIngredientPreferences.AddRange(ingredientPreferences);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<WeeklyProgressSummaryResult>> GetWeeklyHistory(int userId, int weeks = 8)
    {
        var normalizedWeeks = Math.Max(1, weeks);
        var currentWeekStart = WeekDateHelper.GetCurrentWeekStart();
        var rangeStart = currentWeekStart.AddDays(-(normalizedWeeks - 1) * 7);
        var rangeEnd = currentWeekStart.AddDays(7);

        var mealPlans = await _dbContext.MealPlans
            .Where(mp => mp.UserId == userId && mp.Date >= rangeStart && mp.Date < rangeEnd)
            .Include(mp => mp.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .ToListAsync();

        return mealPlans
            .GroupBy(mealPlan => WeekDateHelper.GetWeekStart(mealPlan.Date))
            .Select(group =>
            {
                var meals = group.SelectMany(mealPlan => mealPlan.Meals).ToList();
                var nutrition = meals.Aggregate(
                    new NutritionSummaryResult(),
                    (total, meal) => AddNutrition(total, MealPlanMath.CalculateMealNutrition(meal)));

                return new WeeklyProgressSummaryResult
                {
                    WeekStart = group.Key,
                    WeekEnd = group.Key.AddDays(6),
                    MealsCount = meals.Count,
                    DaysWithMeals = group.Count(mealPlan => mealPlan.Meals.Count > 0),
                    TotalNutrition = nutrition
                };
            })
            .OrderByDescending(summary => summary.WeekStart)
            .ToList();
    }

    public async Task<IEnumerable<MealPlan>> GetAllMealPlans(int userId)
    {
        return await _dbContext.MealPlans
            .Where(mp => mp.UserId == userId)
            .Include(mp => mp.User)
            .Include(mp => mp.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .OrderBy(mp => mp.Date)
            .ToListAsync();
    }

    public async Task<MealPlan?> GetMealPlanById(int id, int userId)
    {
        return await _dbContext.MealPlans
            .Where(mp => mp.UserId == userId)
            .Include(mp => mp.User)
            .Include(mp => mp.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }

    public async Task<MealPlan> CreateMealPlan(int userId, MealPlan mealPlan)
    {
        var targetDate = mealPlan.Date.Date;
        var existingMealPlan = await _dbContext.MealPlans
            .FirstOrDefaultAsync(mp =>
                mp.UserId == userId &&
                mp.Date >= targetDate &&
                mp.Date < targetDate.AddDays(1));
        if (existingMealPlan is not null)
        {
            return existingMealPlan;
        }

        mealPlan.UserId = userId;
        mealPlan.Date = targetDate;
        mealPlan.CreatedAt = DateTime.UtcNow;
        _dbContext.MealPlans.Add(mealPlan);
        await _dbContext.SaveChangesAsync();
        return mealPlan;
    }

    public async Task<bool> UpdateMealPlan(int userId, MealPlan mealPlan)
    {
        var existingMealPlan = await _dbContext.MealPlans
            .FirstOrDefaultAsync(mp => mp.Id == mealPlan.Id && mp.UserId == userId);
        if (existingMealPlan is null)
        {
            return false;
        }

        existingMealPlan.Date = mealPlan.Date.Date;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMealPlan(int id, int userId)
    {
        var mealPlan = await _dbContext.MealPlans
            .FirstOrDefaultAsync(mp => mp.Id == id && mp.UserId == userId);
        if (mealPlan is null)
        {
            return false;
        }

        _dbContext.MealPlans.Remove(mealPlan);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Meal?> AddMealToPlan(int mealPlanId, int userId, bool isAdmin, Meal meal)
    {
        var mealPlanExists = await _dbContext.MealPlans
            .AnyAsync(mp => mp.Id == mealPlanId && mp.UserId == userId);
        if (!mealPlanExists)
        {
            return null;
        }

        var preferences = await GetStoredPlannerPreferencesSnapshot(userId);
        var recipe = await _dbContext.Recipes
            .Include(currentRecipe => currentRecipe.RecipeIngredients)
            .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .FirstOrDefaultAsync(currentRecipe => currentRecipe.Id == meal.RecipeId);
        if (recipe is null)
        {
            return null;
        }

        var canUseRecipe = isAdmin
            || recipe.ApprovalStatus == ApprovalStatus.Approved
            || recipe.OwnerId == userId;
        if (!canUseRecipe || IsRecipeBlockedForPreferences(recipe, preferences))
        {
            return null;
        }

        var duplicateMealTypeExists = await _dbContext.Meals
            .AnyAsync(existingMeal => existingMeal.MealPlanId == mealPlanId && existingMeal.MealType == meal.MealType);
        if (duplicateMealTypeExists)
        {
            return null;
        }

        meal.MealPlanId = mealPlanId;
        meal.RecipeId = recipe.Id;
        meal.CreatedAt = DateTime.UtcNow;
        if (meal.PortionMultiplier <= 0)
        {
            meal.PortionMultiplier = 1.0;
        }
        else
        {
            meal.PortionMultiplier = Math.Clamp(meal.PortionMultiplier, 0.5, 3.0);
        }

        _dbContext.Meals.Add(meal);
        await _dbContext.SaveChangesAsync();
        return meal;
    }

    public async Task<bool> SwapMeal(int mealId, int userId)
    {
        var meal = await _dbContext.Meals
            .Include(currentMeal => currentMeal.MealPlan)
            .Include(currentMeal => currentMeal.Recipe)
            .ThenInclude(recipe => recipe.RecipeIngredients)
            .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .FirstOrDefaultAsync(currentMeal => currentMeal.Id == mealId && currentMeal.MealPlan.UserId == userId);
        if (meal is null)
        {
            return false;
        }

        var preferences = await GetStoredPlannerPreferencesSnapshot(userId);
        var weekStart = WeekDateHelper.GetWeekStart(meal.MealPlan.Date);
        var weekEnd = weekStart.AddDays(7);
        var weeklyMealPlans = await _dbContext.MealPlans
            .Where(mealPlan => mealPlan.UserId == userId && mealPlan.Date >= weekStart && mealPlan.Date < weekEnd)
            .Include(mealPlan => mealPlan.Meals)
            .ThenInclude(currentMeal => currentMeal.Recipe)
            .ThenInclude(recipe => recipe.RecipeIngredients)
            .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .ToListAsync();

        var dailyMealTypes = GetMealTypesForDay(preferences.MealsPerDay);
        var mealShares = MealPlanMath.GetMealCalorieShares(dailyMealTypes.Length);
        var mealIndex = GetMealIndex(dailyMealTypes, meal.MealType);
        var targetNutrition = MealPlanMath.CalculateMealNutritionTarget(
            preferences.DailyNutritionTarget,
            mealShares[mealIndex]);

        var usageCounts = weeklyMealPlans
            .SelectMany(mealPlan => mealPlan.Meals)
            .Where(existingMeal => existingMeal.Id != meal.Id)
            .GroupBy(existingMeal => existingMeal.RecipeId)
            .ToDictionary(group => group.Key, group => group.Count());

        var usedRecipeIdsForDay = weeklyMealPlans
            .Where(mealPlan => mealPlan.Date.Date == meal.MealPlan.Date.Date)
            .SelectMany(mealPlan => mealPlan.Meals)
            .Where(existingMeal => existingMeal.Id != meal.Id)
            .Select(existingMeal => existingMeal.RecipeId)
            .ToHashSet();

        var previousRecipeId = weeklyMealPlans
            .Where(mealPlan => mealPlan.Date.Date < meal.MealPlan.Date.Date)
            .OrderByDescending(mealPlan => mealPlan.Date)
            .SelectMany(mealPlan => mealPlan.Meals.Where(existingMeal => existingMeal.MealType == meal.MealType))
            .Select(existingMeal => (int?)existingMeal.RecipeId)
            .FirstOrDefault();

        var candidates = await GetPlanningCandidates(userId, preferences);
        var selectedMeal = SelectRecipeForMeal(
            candidates,
            meal.MealType,
            targetNutrition,
            usageCounts,
            usedRecipeIdsForDay,
            previousRecipeId,
            [meal.RecipeId]);

        meal.RecipeId = selectedMeal.Recipe.Id;
        meal.PortionMultiplier = selectedMeal.PortionMultiplier;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<WeeklyMealPlanResult> GetWeeklyPlan(int userId, DateTime? weekStart = null)
    {
        var startDate = weekStart.HasValue
            ? WeekDateHelper.GetWeekStart(weekStart.Value)
            : WeekDateHelper.GetCurrentWeekStart();
        var endDate = startDate.AddDays(7);

        var mealPlans = await _dbContext.MealPlans
            .Where(mp => mp.UserId == userId && mp.Date >= startDate && mp.Date < endDate)
            .Include(mp => mp.Meals)
            .ThenInclude(m => m.Recipe)
            .ThenInclude(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .ToListAsync();

        var dailyNutrition = mealPlans
            .SelectMany(mp => mp.Meals.Select(meal => new
            {
                Date = mp.Date.Date,
                Nutrition = MealPlanMath.CalculateMealNutrition(meal)
            }))
            .GroupBy(entry => entry.Date)
            .Select(group => new DailyNutritionResult
            {
                Date = group.Key,
                Nutrition = group.Aggregate(
                    new NutritionSummaryResult(),
                    (total, entry) => AddNutrition(total, entry.Nutrition))
            })
            .OrderBy(result => result.Date)
            .ToList();

        var dailyCalories = dailyNutrition
            .Select(result => new DailyCaloriesResult
            {
                Date = result.Date,
                TotalCalories = result.Nutrition.Calories
            })
            .ToList();

        var weeklyNutrition = dailyNutrition.Aggregate(
            new NutritionSummaryResult(),
            (total, result) => AddNutrition(total, result.Nutrition));
        var mostUsedIngredients = mealPlans
            .SelectMany(mp => mp.Meals)
            .SelectMany(m => m.Recipe.RecipeIngredients.Select(ri => new
            {
                ri.Ingredient.Name,
                QuantityInGrams = MealPlanMath.CalculateIngredientQuantity(m, ri)
            }))
            .GroupBy(x => x.Name)
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
            WeeklyTotalCalories = weeklyNutrition.Calories,
            DailyCalories = dailyCalories,
            DailyNutrition = dailyNutrition,
            WeeklyNutrition = weeklyNutrition,
            MostUsedIngredients = mostUsedIngredients,
            MealPlans = mealPlans
        };
    }

    public async Task<IReadOnlyCollection<PresetMealPlanOptionResult>> GetPresetMealPlans()
    {
        var recipes = await GetPresetRecipes();

        return PresetMealPlanDefinitions
            .Where(plan => plan.Meals.All(meal => recipes.ContainsKey(meal.RecipeId)))
            .Select(plan => new PresetMealPlanOptionResult
            {
                Key = plan.Key,
                Name = plan.Name,
                Description = plan.Description,
                AverageDailyCaloriesAtReferenceWeight = (int)Math.Round(
                    plan.Meals.Sum(meal => recipes[meal.RecipeId].Calories) / 7.0,
                    MidpointRounding.AwayFromZero),
                FeaturedRecipes = plan.Meals
                    .Select(meal => recipes[meal.RecipeId].Name)
                    .Distinct()
                    .Take(3)
                    .ToArray(),
                IsVegetarian = plan.Meals.All(meal => recipes[meal.RecipeId].IsVegetarian)
            })
            .ToList();
    }

    public async Task StartPresetMealPlan(StartPresetMealPlanRequest request)
    {
        var presetPlan = PresetMealPlanDefinitions.FirstOrDefault(plan => plan.Key == request.PresetKey);
        if (presetPlan is null)
        {
            throw new InvalidOperationException("The selected starter plan is no longer available.");
        }

        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
        {
            throw new InvalidOperationException("The selected user does not exist.");
        }

        var requiredRecipeIds = presetPlan.Meals.Select(meal => meal.RecipeId).Distinct().ToList();
        var preferences = await GetStoredPlannerPreferencesSnapshot(request.UserId);
        var requiredRecipes = await _dbContext.Recipes
            .Where(recipe => requiredRecipeIds.Contains(recipe.Id) && recipe.ApprovalStatus == ApprovalStatus.Approved)
            .Include(recipe => recipe.RecipeIngredients)
            .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .ToListAsync();

        if (requiredRecipes.Count != requiredRecipeIds.Count)
        {
            throw new InvalidOperationException("This starter plan cannot be started because one or more required recipes are missing.");
        }

        if (requiredRecipes.Any(recipe => IsRecipeBlockedForPreferences(recipe, preferences)))
        {
            throw new InvalidOperationException("This starter plan includes ingredients from your excluded or allergy list.");
        }

        var weekStart = WeekDateHelper.GetWeekStart(request.WeekStart);
        var portionMultiplier = MealPlanMath.CalculatePortionMultiplier(request.BodyWeightKg);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        var mealPlansByDate = await PrepareWeekForReplacement(request.UserId, weekStart);

        var mealsToAdd = presetPlan.Meals
            .Select(meal => new Meal
            {
                MealPlanId = mealPlansByDate[weekStart.AddDays(meal.DayOffset).Date].Id,
                RecipeId = meal.RecipeId,
                MealType = meal.MealType,
                PortionMultiplier = portionMultiplier,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        _dbContext.Meals.AddRange(mealsToAdd);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task GeneratePersonalizedMealPlan(GeneratePersonalizedMealPlanRequest request)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
        {
            throw new InvalidOperationException("The selected user does not exist.");
        }

        var weekStart = WeekDateHelper.GetWeekStart(request.WeekStart);
        var preferences = CreatePlannerPreferencesSnapshot(
            request.MealsPerDay,
            request.BodyWeightKg,
            request.ProteinTargetGrams,
            request.CarbsTargetGrams,
            request.FatTargetGrams,
            request.ExcludedFoods,
            request.ExcludedIngredientIds,
            request.AllergyIngredientIds);
        var candidates = await GetPlanningCandidates(request.UserId, preferences);

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("No recipes match your foods, ingredient exclusions, and allergy preferences.");
        }

        var plannedMeals = BuildGeneratedMeals(candidates, preferences);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        var mealPlansByDate = await PrepareWeekForReplacement(request.UserId, weekStart);

        var mealsToAdd = plannedMeals
            .Select(meal => new Meal
            {
                MealPlanId = mealPlansByDate[weekStart.AddDays(meal.DayOffset).Date].Id,
                RecipeId = meal.RecipeId,
                MealType = meal.MealType,
                PortionMultiplier = meal.PortionMultiplier,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        _dbContext.Meals.AddRange(mealsToAdd);
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task<Dictionary<int, Recipe>> GetPresetRecipes()
    {
        var recipeIds = PresetMealPlanDefinitions
            .SelectMany(plan => plan.Meals)
            .Select(meal => meal.RecipeId)
            .Distinct()
            .ToList();

        return await _dbContext.Recipes
            .Where(recipe => recipeIds.Contains(recipe.Id) && recipe.ApprovalStatus == ApprovalStatus.Approved)
            .ToDictionaryAsync(recipe => recipe.Id);
    }

    private async Task<Dictionary<DateTime, MealPlan>> PrepareWeekForReplacement(int userId, DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(7);
        var existingMealPlans = await _dbContext.MealPlans
            .Where(mp => mp.UserId == userId && mp.Date >= weekStart && mp.Date < weekEnd)
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
            var date = weekStart.AddDays(offset).Date;
            if (mealPlansByDate.ContainsKey(date))
            {
                continue;
            }

            var mealPlan = new MealPlan
            {
                UserId = userId,
                Date = date,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.MealPlans.Add(mealPlan);
            mealPlansByDate[date] = mealPlan;
        }

        await _dbContext.SaveChangesAsync();
        return mealPlansByDate;
    }

    private async Task<List<PlanningRecipeCandidate>> GetPlanningCandidates(int userId, PlannerPreferencesSnapshot preferences)
    {
        var recipes = await _dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.ApprovalStatus == ApprovalStatus.Approved || recipe.OwnerId == userId)
            .Include(recipe => recipe.RecipeIngredients)
            .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .ToListAsync();

        return recipes
            .Select(recipe =>
            {
                var searchText = BuildRecipeSearchText(recipe);
                return new PlanningRecipeCandidate(
                    recipe,
                    searchText,
                    MealPlanMath.CalculateRecipeNutrition(recipe),
                    recipe.RecipeIngredients.Select(recipeIngredient => recipeIngredient.IngredientId).ToHashSet());
            })
            .Where(candidate => !IsRecipeBlockedForPreferences(candidate.SearchText, candidate.IngredientIds, preferences))
            .ToList();
    }

    private async Task<PlannerPreferencesSnapshot> GetStoredPlannerPreferencesSnapshot(int userId)
    {
        var user = await _dbContext.Users
            .Include(currentUser => currentUser.IngredientPreferences)
            .FirstOrDefaultAsync(currentUser => currentUser.Id == userId);
        if (user is null)
        {
            throw new InvalidOperationException("The selected user does not exist.");
        }

        return CreatePlannerPreferencesSnapshot(
            user.PreferredMealsPerDay,
            user.BodyWeightKg,
            user.ProteinTargetGrams,
            user.CarbsTargetGrams,
            user.FatTargetGrams,
            SplitExcludedFoods(user.ExcludedFoods),
            user.IngredientPreferences
                .Where(preference => preference.PreferenceType == UserIngredientPreferenceType.Excluded)
                .Select(preference => preference.IngredientId),
            user.IngredientPreferences
                .Where(preference => preference.PreferenceType == UserIngredientPreferenceType.Allergy)
                .Select(preference => preference.IngredientId));
    }

    private static PlannerPreferencesSnapshot CreatePlannerPreferencesSnapshot(
        int mealsPerDay,
        double bodyWeightKg,
        double proteinTargetGrams,
        double carbsTargetGrams,
        double fatTargetGrams,
        IEnumerable<string> excludedFoods,
        IEnumerable<int> excludedIngredientIds,
        IEnumerable<int> allergyIngredientIds)
    {
        var normalizedWeight = Math.Clamp(bodyWeightKg, 40.0, 180.0);
        var defaultTargets = MealPlanMath.CalculateDefaultDailyMacroTargets(normalizedWeight);
        var normalizedProtein = proteinTargetGrams is >= 40 and <= 300 ? proteinTargetGrams : defaultTargets.ProteinGrams;
        var normalizedCarbs = carbsTargetGrams is >= 20 and <= 500 ? carbsTargetGrams : defaultTargets.CarbsGrams;
        var normalizedFat = fatTargetGrams is >= 20 and <= 200 ? fatTargetGrams : defaultTargets.FatGrams;
        var normalizedExcludedFoods = excludedFoods
            .Where(food => !string.IsNullOrWhiteSpace(food))
            .Select(food => food.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();
        var normalizedAllergyIngredientIds = allergyIngredientIds
            .Distinct()
            .ToHashSet();
        var normalizedExcludedIngredientIds = excludedIngredientIds
            .Distinct()
            .Where(ingredientId => !normalizedAllergyIngredientIds.Contains(ingredientId))
            .ToHashSet();
        var blockedIngredientIds = normalizedExcludedIngredientIds
            .Concat(normalizedAllergyIngredientIds)
            .ToHashSet();

        return new PlannerPreferencesSnapshot(
            Math.Clamp(mealsPerDay, 1, 3),
            normalizedWeight,
            new NutritionSummaryResult
            {
                Calories = MealPlanMath.CalculateDailyCaloriesFromMacros(normalizedProtein, normalizedCarbs, normalizedFat),
                ProteinGrams = normalizedProtein,
                CarbsGrams = normalizedCarbs,
                FatGrams = normalizedFat
            },
            normalizedExcludedFoods,
            normalizedExcludedIngredientIds,
            normalizedAllergyIngredientIds,
            blockedIngredientIds);
    }

    private static IReadOnlyCollection<GeneratedMealPlanEntry> BuildGeneratedMeals(
        IReadOnlyCollection<PlanningRecipeCandidate> candidates,
        PlannerPreferencesSnapshot preferences)
    {
        var mealTypes = GetMealTypesForDay(preferences.MealsPerDay);
        var mealShares = MealPlanMath.GetMealCalorieShares(mealTypes.Length);
        var usageCounts = new Dictionary<int, int>();
        var previousRecipeByMealType = new Dictionary<MealType, int>();
        var plannedMeals = new List<GeneratedMealPlanEntry>();

        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var usedRecipeIdsForDay = new HashSet<int>();

            for (var mealIndex = 0; mealIndex < mealTypes.Length; mealIndex++)
            {
                var mealType = mealTypes[mealIndex];
                var targetNutrition = MealPlanMath.CalculateMealNutritionTarget(
                    preferences.DailyNutritionTarget,
                    mealShares[mealIndex]);
                var selectedMeal = SelectRecipeForMeal(
                    candidates,
                    mealType,
                    targetNutrition,
                    usageCounts,
                    usedRecipeIdsForDay,
                    previousRecipeByMealType.TryGetValue(mealType, out var previousRecipeId) ? previousRecipeId : null);

                plannedMeals.Add(new GeneratedMealPlanEntry(
                    dayOffset,
                    mealType,
                    selectedMeal.Recipe.Id,
                    selectedMeal.PortionMultiplier,
                    selectedMeal.Nutrition));

                usedRecipeIdsForDay.Add(selectedMeal.Recipe.Id);
                usageCounts[selectedMeal.Recipe.Id] = usageCounts.GetValueOrDefault(selectedMeal.Recipe.Id) + 1;
                previousRecipeByMealType[mealType] = selectedMeal.Recipe.Id;
            }
        }

        return plannedMeals;
    }

    private static MealType[] GetMealTypesForDay(int mealsPerDay)
    {
        return mealsPerDay switch
        {
            1 => [MealType.Lunch],
            2 => [MealType.Lunch, MealType.Dinner],
            _ => [MealType.Breakfast, MealType.Lunch, MealType.Dinner]
        };
    }

    private static int GetMealIndex(MealType[] mealTypes, MealType mealType)
    {
        var index = Array.IndexOf(mealTypes, mealType);
        if (index >= 0)
        {
            return index;
        }

        return mealType switch
        {
            MealType.Breakfast => 0,
            MealType.Lunch => Math.Min(1, mealTypes.Length - 1),
            MealType.Dinner => mealTypes.Length - 1,
            _ => 0
        };
    }

    private static SelectedMealPlan SelectRecipeForMeal(
        IReadOnlyCollection<PlanningRecipeCandidate> candidates,
        MealType mealType,
        NutritionSummaryResult targetNutrition,
        IReadOnlyDictionary<int, int> usageCounts,
        IReadOnlySet<int> usedRecipeIdsForDay,
        int? previousRecipeId,
        IReadOnlyCollection<int>? excludedRecipeIds = null)
    {
        var excludedRecipeIdSet = excludedRecipeIds?.ToHashSet() ?? [];
        var rankedCandidates = candidates
            .Where(candidate => !excludedRecipeIdSet.Contains(candidate.Recipe.Id))
            .Select(candidate => new
            {
                Candidate = candidate,
                PortionMultiplier = MealPlanMath.CalculatePortionMultiplierForNutrition(targetNutrition, candidate.Nutrition),
                ScaledNutrition = MealPlanMath.ScaleNutrition(
                    candidate.Nutrition,
                    MealPlanMath.CalculatePortionMultiplierForNutrition(targetNutrition, candidate.Nutrition)),
                Score = GetMealTypeScore(candidate, mealType),
                WasUsedToday = usedRecipeIdsForDay.Contains(candidate.Recipe.Id),
                WasUsedLastTime = previousRecipeId.HasValue && previousRecipeId.Value == candidate.Recipe.Id,
                UsageCount = usageCounts.GetValueOrDefault(candidate.Recipe.Id),
                NutritionDistance = CalculateNutritionDistance(
                    MealPlanMath.ScaleNutrition(
                        candidate.Nutrition,
                        MealPlanMath.CalculatePortionMultiplierForNutrition(targetNutrition, candidate.Nutrition)),
                    targetNutrition)
            })
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.WasUsedToday)
            .ThenBy(candidate => candidate.WasUsedLastTime)
            .ThenBy(candidate => candidate.UsageCount)
            .ThenBy(candidate => candidate.NutritionDistance)
            .ThenBy(candidate => candidate.Candidate.Recipe.CookingTime)
            .ThenBy(candidate => candidate.Candidate.Recipe.Name)
            .ToList();

        var strongestCandidates = rankedCandidates.Where(candidate => candidate.Score > 0).ToList();
        var selected = strongestCandidates.Count > 0 ? strongestCandidates[0] : rankedCandidates.FirstOrDefault();
        if (selected is null)
        {
            throw new InvalidOperationException("No recipes are available for the generated meal plan.");
        }

        return new SelectedMealPlan(
            selected.Candidate.Recipe,
            selected.PortionMultiplier,
            selected.ScaledNutrition);
    }

    private static int GetMealTypeScore(PlanningRecipeCandidate candidate, MealType mealType)
    {
        var hasBreakfastKeyword = BreakfastKeywords.Any(keyword => candidate.SearchText.Contains(keyword, StringComparison.Ordinal));
        var hasHeartyKeyword = HeartyMealKeywords.Any(keyword => candidate.SearchText.Contains(keyword, StringComparison.Ordinal));

        return mealType switch
        {
            MealType.Breakfast => GetBreakfastScore(candidate, hasBreakfastKeyword),
            MealType.Lunch => GetLunchScore(candidate, hasBreakfastKeyword, hasHeartyKeyword),
            MealType.Dinner => GetDinnerScore(candidate, hasBreakfastKeyword, hasHeartyKeyword),
            _ => 0
        };
    }

    private static int GetBreakfastScore(PlanningRecipeCandidate candidate, bool hasBreakfastKeyword)
    {
        var score = 0;
        if (hasBreakfastKeyword)
        {
            score += 4;
        }

        if (candidate.Recipe.CookingTime <= 15)
        {
            score += 2;
        }

        if (candidate.Recipe.Calories <= 450)
        {
            score += 2;
        }

        if (candidate.Recipe.IsVegetarian)
        {
            score += 1;
        }

        if (candidate.Nutrition.ProteinGrams >= 15)
        {
            score += 1;
        }

        return score;
    }

    private static int GetLunchScore(PlanningRecipeCandidate candidate, bool hasBreakfastKeyword, bool hasHeartyKeyword)
    {
        var score = 0;
        if (!hasBreakfastKeyword)
        {
            score += 2;
        }

        if (candidate.Recipe.Calories is >= 350 and <= 650)
        {
            score += 2;
        }

        if (candidate.Recipe.CookingTime <= 35)
        {
            score += 1;
        }

        if (hasHeartyKeyword)
        {
            score += 1;
        }

        if (candidate.Nutrition.ProteinGrams >= 25)
        {
            score += 1;
        }

        return score;
    }

    private static int GetDinnerScore(PlanningRecipeCandidate candidate, bool hasBreakfastKeyword, bool hasHeartyKeyword)
    {
        var score = 0;
        if (!hasBreakfastKeyword)
        {
            score += 2;
        }

        if (candidate.Recipe.Calories >= 450)
        {
            score += 2;
        }

        if (candidate.Recipe.CookingTime >= 20)
        {
            score += 1;
        }

        if (hasHeartyKeyword)
        {
            score += 1;
        }

        if (candidate.Nutrition.ProteinGrams >= 25)
        {
            score += 1;
        }

        return score;
    }

    private static bool IsRecipeBlockedForPreferences(Recipe recipe, PlannerPreferencesSnapshot preferences)
    {
        return IsRecipeBlockedForPreferences(
            BuildRecipeSearchText(recipe),
            recipe.RecipeIngredients.Select(recipeIngredient => recipeIngredient.IngredientId),
            preferences);
    }

    private static bool IsRecipeBlockedForPreferences(
        string searchText,
        IEnumerable<int> ingredientIds,
        PlannerPreferencesSnapshot preferences)
    {
        if (preferences.ExcludedFoodTerms.Any(term => searchText.Contains(term, StringComparison.Ordinal)))
        {
            return true;
        }

        return ingredientIds.Any(ingredientId => preferences.BlockedIngredientIds.Contains(ingredientId));
    }

    private static double CalculateNutritionDistance(NutritionSummaryResult actual, NutritionSummaryResult target)
    {
        return Math.Abs(actual.Calories - target.Calories) / 25.0
               + (Math.Abs(actual.ProteinGrams - target.ProteinGrams) * 2.0)
               + Math.Abs(actual.CarbsGrams - target.CarbsGrams)
               + (Math.Abs(actual.FatGrams - target.FatGrams) * 1.5);
    }

    private static NutritionSummaryResult AddNutrition(NutritionSummaryResult left, NutritionSummaryResult right)
    {
        return new NutritionSummaryResult
        {
            Calories = left.Calories + right.Calories,
            ProteinGrams = Math.Round(left.ProteinGrams + right.ProteinGrams, 1, MidpointRounding.AwayFromZero),
            CarbsGrams = Math.Round(left.CarbsGrams + right.CarbsGrams, 1, MidpointRounding.AwayFromZero),
            FatGrams = Math.Round(left.FatGrams + right.FatGrams, 1, MidpointRounding.AwayFromZero)
        };
    }

    private static IReadOnlyCollection<string> SplitExcludedFoods(string? excludedFoods)
    {
        return (excludedFoods ?? string.Empty)
            .Split([',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private static string BuildRecipeSearchText(Recipe recipe)
    {
        return string.Join(
                ' ',
                new[]
                {
                    recipe.Name,
                    recipe.Description ?? string.Empty,
                    recipe.Instructions ?? string.Empty,
                    string.Join(' ', recipe.RecipeIngredients.Select(ingredient => ingredient.Ingredient.Name))
                })
            .ToLowerInvariant();
    }

    private sealed record PresetMealPlanDefinition(
        string Key,
        string Name,
        string Description,
        IReadOnlyCollection<PresetMealPlanEntry> Meals);

    private sealed record PresetMealPlanEntry(int DayOffset, MealType MealType, int RecipeId);
    private sealed record PlanningRecipeCandidate(
        Recipe Recipe,
        string SearchText,
        NutritionSummaryResult Nutrition,
        IReadOnlySet<int> IngredientIds);
    private sealed record SelectedMealPlan(Recipe Recipe, double PortionMultiplier, NutritionSummaryResult Nutrition);
    private sealed record GeneratedMealPlanEntry(
        int DayOffset,
        MealType MealType,
        int RecipeId,
        double PortionMultiplier,
        NutritionSummaryResult Nutrition);
    private sealed record PlannerPreferencesSnapshot(
        int MealsPerDay,
        double BodyWeightKg,
        NutritionSummaryResult DailyNutritionTarget,
        IReadOnlyCollection<string> ExcludedFoodTerms,
        IReadOnlySet<int> ExcludedIngredientIds,
        IReadOnlySet<int> AllergyIngredientIds,
        IReadOnlySet<int> BlockedIngredientIds);
}
