using MealPlannerApp.Dtos.MealPlans;
using MealPlannerApp.Infrastructure;
using MealPlannerApp.Models;
using MealPlannerApp.Services;
using MealPlannerApp.Services.Interfaces;
using MealPlannerApp.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MealPlannerApp.Controllers;

[Authorize]
public class MealPlansController : Controller
{
    private readonly IMealPlanService _mealPlanService;
    private readonly IRecipeService _recipeService;
    private readonly IIngredientService _ingredientService;

    public MealPlansController(IMealPlanService mealPlanService, IRecipeService recipeService, IIngredientService ingredientService)
    {
        _mealPlanService = mealPlanService;
        _recipeService = recipeService;
        _ingredientService = ingredientService;
    }

    public async Task<IActionResult> Index()
    {
        var mealPlans = await _mealPlanService.GetAllMealPlans(User.GetRequiredUserId());
        var dto = mealPlans.Select(MapToDto).ToList();
        return View(dto);
    }

    public async Task<IActionResult> Details(int id)
    {
        var mealPlan = await _mealPlanService.GetMealPlanById(id, User.GetRequiredUserId());
        if (mealPlan is null)
        {
            return NotFound();
        }

        return View(MapToDto(mealPlan));
    }

    public IActionResult Create()
    {
        return View(new MealPlanDto { Date = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MealPlanDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await _mealPlanService.CreateMealPlan(User.GetRequiredUserId(), MapToEntity(dto));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var mealPlan = await _mealPlanService.GetMealPlanById(id, User.GetRequiredUserId());
        if (mealPlan is null)
        {
            return NotFound();
        }

        return View(MapToDto(mealPlan));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MealPlanDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var updated = await _mealPlanService.UpdateMealPlan(User.GetRequiredUserId(), MapToEntity(dto));
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var mealPlan = await _mealPlanService.GetMealPlanById(id, User.GetRequiredUserId());
        if (mealPlan is null)
        {
            return NotFound();
        }

        return View(MapToDto(mealPlan));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _mealPlanService.DeleteMealPlan(id, User.GetRequiredUserId());
        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Weekly(DateTime? weekStart = null, DateTime? selectedDate = null)
    {
        var model = await BuildWeeklyPlannerModel(weekStart, selectedDate);
        return View(model);
    }

    public async Task<IActionResult> History()
    {
        var history = await _mealPlanService.GetWeeklyHistory(User.GetRequiredUserId());
        var weeks = history.Select(summary => new WeeklyProgressSummaryDto
        {
            WeekStart = summary.WeekStart,
            WeekEnd = summary.WeekEnd,
            MealsCount = summary.MealsCount,
            DaysWithMeals = summary.DaysWithMeals,
            TotalCalories = summary.TotalNutrition.Calories,
            AverageDailyCalories = (int)Math.Round(summary.TotalNutrition.Calories / 7.0, MidpointRounding.AwayFromZero),
            TotalNutrition = MapNutrition(summary.TotalNutrition),
            AverageDailyNutrition = new NutritionSummaryDto
            {
                Calories = (int)Math.Round(summary.TotalNutrition.Calories / 7.0, MidpointRounding.AwayFromZero),
                ProteinGrams = Math.Round(summary.TotalNutrition.ProteinGrams / 7.0, 1, MidpointRounding.AwayFromZero),
                CarbsGrams = Math.Round(summary.TotalNutrition.CarbsGrams / 7.0, 1, MidpointRounding.AwayFromZero),
                FatGrams = Math.Round(summary.TotalNutrition.FatGrams / 7.0, 1, MidpointRounding.AwayFromZero)
            }
        }).ToList();

        var averageDailyNutrition = weeks.Count == 0
            ? new NutritionSummaryDto()
            : new NutritionSummaryDto
            {
                Calories = (int)Math.Round(weeks.Average(week => week.AverageDailyCalories), MidpointRounding.AwayFromZero),
                ProteinGrams = Math.Round(weeks.Average(week => week.AverageDailyNutrition.ProteinGrams), 1, MidpointRounding.AwayFromZero),
                CarbsGrams = Math.Round(weeks.Average(week => week.AverageDailyNutrition.CarbsGrams), 1, MidpointRounding.AwayFromZero),
                FatGrams = Math.Round(weeks.Average(week => week.AverageDailyNutrition.FatGrams), 1, MidpointRounding.AwayFromZero)
            };

        return View(new MealPlanHistoryDto
        {
            Weeks = weeks,
            AverageDailyCalories = averageDailyNutrition.Calories,
            AverageDailyNutrition = averageDailyNutrition
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartPreset([Bind(Prefix = "StartPresetPlan")] StartPresetMealPlanDto dto, DateTime? selectedDate = null)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = await BuildWeeklyPlannerModel(dto.WeekStart, selectedDate, startPresetPlan: dto);
            return View(nameof(Weekly), invalidModel);
        }

        try
        {
            await _mealPlanService.StartPresetMealPlan(new StartPresetMealPlanRequest
            {
                UserId = User.GetRequiredUserId(),
                WeekStart = dto.WeekStart,
                PresetKey = dto.PresetKey,
                BodyWeightKg = dto.BodyWeightKg
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var errorModel = await BuildWeeklyPlannerModel(dto.WeekStart, selectedDate, startPresetPlan: dto);
            return View(nameof(Weekly), errorModel);
        }

        TempData["SuccessMessage"] = "Starter plan added to the selected week.";
        return RedirectToAction(nameof(Weekly), new
        {
            weekStart = WeekDateHelper.GetWeekStart(dto.WeekStart).ToString("yyyy-MM-dd"),
            selectedDate = NormalizeSelectedDate(selectedDate, WeekDateHelper.GetWeekStart(dto.WeekStart)).ToString("yyyy-MM-dd")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate([Bind(Prefix = "GeneratePlan")] GeneratePersonalizedMealPlanDto dto, DateTime? selectedDate = null)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = await BuildWeeklyPlannerModel(dto.WeekStart, selectedDate, generatePlan: dto);
            return View(nameof(Weekly), invalidModel);
        }

        try
        {
            var excludedFoods = ParseExcludedFoods(dto.ExcludedFoods);

            await _mealPlanService.GeneratePersonalizedMealPlan(new GeneratePersonalizedMealPlanRequest
            {
                UserId = User.GetRequiredUserId(),
                WeekStart = dto.WeekStart,
                MealsPerDay = dto.MealsPerDay,
                BodyWeightKg = dto.BodyWeightKg,
                ProteinTargetGrams = dto.ProteinTargetGrams,
                CarbsTargetGrams = dto.CarbsTargetGrams,
                FatTargetGrams = dto.FatTargetGrams,
                ExcludedFoods = excludedFoods,
                ExcludedIngredientIds = dto.ExcludedIngredientIds,
                AllergyIngredientIds = dto.AllergyIngredientIds
            });

            await _mealPlanService.SavePlannerPreferences(new SavePlannerPreferencesRequest
            {
                UserId = User.GetRequiredUserId(),
                MealsPerDay = dto.MealsPerDay,
                BodyWeightKg = dto.BodyWeightKg,
                ProteinTargetGrams = dto.ProteinTargetGrams,
                CarbsTargetGrams = dto.CarbsTargetGrams,
                FatTargetGrams = dto.FatTargetGrams,
                ExcludedFoods = dto.ExcludedFoods,
                ExcludedIngredientIds = dto.ExcludedIngredientIds,
                AllergyIngredientIds = dto.AllergyIngredientIds
            });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var errorModel = await BuildWeeklyPlannerModel(dto.WeekStart, selectedDate, generatePlan: dto);
            return View(nameof(Weekly), errorModel);
        }

        TempData["SuccessMessage"] = "Personalized week generated successfully.";
        return RedirectToAction(nameof(Weekly), new
        {
            weekStart = WeekDateHelper.GetWeekStart(dto.WeekStart).ToString("yyyy-MM-dd"),
            selectedDate = NormalizeSelectedDate(selectedDate, WeekDateHelper.GetWeekStart(dto.WeekStart)).ToString("yyyy-MM-dd")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SwapMeal(int mealId, DateTime weekStart, DateTime? selectedDate = null)
    {
        try
        {
            var swapped = await _mealPlanService.SwapMeal(mealId, User.GetRequiredUserId());
            if (!swapped)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Meal swapped with a new match.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Weekly), new
        {
            weekStart = WeekDateHelper.GetWeekStart(weekStart).ToString("yyyy-MM-dd"),
            selectedDate = NormalizeSelectedDate(selectedDate, WeekDateHelper.GetWeekStart(weekStart)).ToString("yyyy-MM-dd")
        });
    }

    public async Task<IActionResult> AddMeal(DateTime? weekStart = null)
    {
        var userId = User.GetRequiredUserId();
        var result = await _mealPlanService.GetWeeklyPlan(userId, weekStart);
        for (var offset = 0; offset < 7; offset++)
        {
            await _mealPlanService.CreateMealPlan(userId, new MealPlan
            {
                Date = result.WeekStart.AddDays(offset)
            });
        }

        result = await _mealPlanService.GetWeeklyPlan(userId, result.WeekStart);

        var model = new AddMealDto
        {
            WeekStart = result.WeekStart
        };

        await PopulateAddMealLookups(result.WeekStart);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMeal(AddMealDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAddMealLookups(dto.WeekStart);
            return View(dto);
        }

        var meal = await _mealPlanService.AddMealToPlan(dto.MealPlanId, User.GetRequiredUserId(), User.IsInRole(ApplicationRoles.Admin), new Meal
        {
            RecipeId = dto.RecipeId,
            MealType = dto.MealType,
            PortionMultiplier = dto.PortionMultiplier
        });
        if (meal is null)
        {
            ModelState.AddModelError(string.Empty, "Choose a valid day and recipe, avoid blocked ingredients, and keep one meal per type for each day.");
            await PopulateAddMealLookups(dto.WeekStart);
            return View(dto);
        }

        return RedirectToAction(nameof(Weekly), new { weekStart = dto.WeekStart.ToString("yyyy-MM-dd") });
    }

    private async Task PopulateAddMealLookups(DateTime weekStart)
    {
        var weeklyPlan = await _mealPlanService.GetWeeklyPlan(User.GetRequiredUserId(), weekStart);
        var preferences = await _mealPlanService.GetPlannerPreferences(User.GetRequiredUserId());
        var blockedIngredientIds = preferences.ExcludedIngredientIds
            .Concat(preferences.AllergyIngredientIds)
            .ToHashSet();
        var excludedFoodTerms = ParseExcludedFoods(preferences.ExcludedFoods)
            .Select(term => term.ToLowerInvariant())
            .ToArray();
        var recipes = await _recipeService.GetAllRecipes(User.GetRequiredUserId(), User.IsInRole(ApplicationRoles.Admin));
        var filteredRecipes = recipes
            .Where(recipe =>
                !recipe.RecipeIngredients.Any(recipeIngredient => blockedIngredientIds.Contains(recipeIngredient.IngredientId)) &&
                !excludedFoodTerms.Any(term =>
                {
                    var recipeSearchText = string.Join(
                            ' ',
                            new[]
                            {
                                recipe.Name,
                                recipe.Description ?? string.Empty,
                                recipe.Instructions ?? string.Empty,
                                string.Join(' ', recipe.RecipeIngredients.Select(recipeIngredient => recipeIngredient.Ingredient.Name))
                            })
                        .ToLowerInvariant();

                    return recipeSearchText.Contains(term, StringComparison.Ordinal);
                }))
            .ToList();

        ViewBag.MealPlans = weeklyPlan.MealPlans
            .OrderBy(mp => mp.Date)
            .Select(mp => new SelectListItem
            {
                Value = mp.Id.ToString(),
                Text = $"{mp.Date:ddd, dd MMM yyyy}"
            })
            .ToList();

        ViewBag.Recipes = filteredRecipes
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.Name} ({r.Calories} kcal base)"
            })
            .ToList();
    }

    private async Task<WeeklyMealPlannerDto> BuildWeeklyPlannerModel(
        DateTime? weekStart,
        DateTime? selectedDate,
        StartPresetMealPlanDto? startPresetPlan = null,
        GeneratePersonalizedMealPlanDto? generatePlan = null)
    {
        var result = await _mealPlanService.GetWeeklyPlan(User.GetRequiredUserId(), weekStart);
        var plannerPreferences = await _mealPlanService.GetPlannerPreferences(User.GetRequiredUserId());
        var ingredientOptions = (await _ingredientService.GetAllIngredients())
            .Select(ingredient => new PlannerIngredientOptionDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name
            })
            .ToList();
        var presetPlans = (await _mealPlanService.GetPresetMealPlans())
            .Select(plan => new PresetMealPlanOptionDto
            {
                Key = plan.Key,
                Name = plan.Name,
                Description = plan.Description,
                AverageDailyCaloriesAtReferenceWeight = plan.AverageDailyCaloriesAtReferenceWeight,
                FeaturedRecipes = plan.FeaturedRecipes,
                IsVegetarian = plan.IsVegetarian
            })
            .ToList();

        var startPresetModel = startPresetPlan ?? new StartPresetMealPlanDto
        {
            WeekStart = result.WeekStart,
            BodyWeightKg = plannerPreferences.BodyWeightKg,
            PresetKey = presetPlans.FirstOrDefault()?.Key ?? string.Empty
        };

        if (startPresetPlan is not null)
        {
            startPresetModel.WeekStart = startPresetPlan.WeekStart == default
                ? result.WeekStart
                : WeekDateHelper.GetWeekStart(startPresetPlan.WeekStart);
            if (string.IsNullOrWhiteSpace(startPresetModel.PresetKey))
            {
                startPresetModel.PresetKey = presetPlans.FirstOrDefault()?.Key ?? string.Empty;
            }
        }

        var generatePlanModel = generatePlan ?? BuildDefaultGeneratePlan(plannerPreferences, result.WeekStart);
        if (generatePlan is not null)
        {
            generatePlanModel.WeekStart = generatePlan.WeekStart == default
                ? result.WeekStart
                : WeekDateHelper.GetWeekStart(generatePlan.WeekStart);
        }

        return BuildWeeklyPlannerDto(
            result,
            ingredientOptions,
            plannerPreferences,
            presetPlans,
            generatePlanModel,
            startPresetModel,
            ResolveSelectedDate(result, selectedDate));
    }

    private static WeeklyMealPlannerDto BuildWeeklyPlannerDto(
        WeeklyMealPlanResult result,
        IReadOnlyCollection<PlannerIngredientOptionDto> ingredientOptions,
        PlannerPreferencesResult plannerPreferences,
        IReadOnlyCollection<PresetMealPlanOptionDto> presetPlans,
        GeneratePersonalizedMealPlanDto generatePlan,
        StartPresetMealPlanDto startPresetPlan,
        DateTime selectedDate)
    {
        var dayMap = result.MealPlans
            .GroupBy(mp => mp.Date.Date)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(mp => mp.Meals).OrderBy(m => m.MealType).ToList());
        var dailyNutritionMap = result.DailyNutrition.ToDictionary(day => day.Date.Date, day => day.Nutrition);
        var dailyNutritionTarget = new NutritionSummaryDto
        {
            Calories = MealPlanMath.CalculateDailyCaloriesFromMacros(
                plannerPreferences.ProteinTargetGrams,
                plannerPreferences.CarbsTargetGrams,
                plannerPreferences.FatTargetGrams),
            ProteinGrams = plannerPreferences.ProteinTargetGrams,
            CarbsGrams = plannerPreferences.CarbsTargetGrams,
            FatGrams = plannerPreferences.FatTargetGrams
        };

        var days = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = result.WeekStart.AddDays(offset).Date;
                var meals = dayMap.TryGetValue(date, out var dayMeals)
                    ? dayMeals.Select(MapWeeklyMeal).ToList()
                    : new List<WeeklyMealItemDto>();

                return new WeeklyDayDto
                {
                    Date = date,
                    TotalCalories = meals.Sum(m => m.Calories),
                    Nutrition = dailyNutritionMap.TryGetValue(date, out var dayNutrition)
                        ? MapNutrition(dayNutrition)
                        : new NutritionSummaryDto(),
                    Meals = meals
                };
            })
            .ToList();

        return new WeeklyMealPlannerDto
        {
            WeekStart = result.WeekStart,
            WeekEnd = result.WeekEnd,
            SelectedDate = selectedDate,
            WeeklyTotalCalories = result.WeeklyTotalCalories,
            WeeklyNutrition = MapNutrition(result.WeeklyNutrition),
            DailyNutritionTarget = dailyNutritionTarget,
            WeeklyNutritionTarget = new NutritionSummaryDto
            {
                Calories = dailyNutritionTarget.Calories * 7,
                ProteinGrams = Math.Round(dailyNutritionTarget.ProteinGrams * 7, 1, MidpointRounding.AwayFromZero),
                CarbsGrams = Math.Round(dailyNutritionTarget.CarbsGrams * 7, 1, MidpointRounding.AwayFromZero),
                FatGrams = Math.Round(dailyNutritionTarget.FatGrams * 7, 1, MidpointRounding.AwayFromZero)
            },
            Days = days,
            MostUsedIngredients = result.MostUsedIngredients
                .Select(i => new MostUsedIngredientDto
                {
                    Name = i.Name,
                    UsageCount = i.UsageCount,
                    TotalQuantityInGrams = i.TotalQuantityInGrams
                })
                .ToList(),
            AvailableIngredients = ingredientOptions,
            PresetPlans = presetPlans,
            GeneratePlan = generatePlan,
            StartPresetPlan = startPresetPlan
        };
    }

    private static DateTime ResolveSelectedDate(WeeklyMealPlanResult result, DateTime? selectedDate)
    {
        if (selectedDate.HasValue)
        {
            return NormalizeSelectedDate(selectedDate, result.WeekStart);
        }

        var today = DateTime.Today;
        if (today >= result.WeekStart.Date && today <= result.WeekEnd.Date)
        {
            return today.Date;
        }

        var firstDayWithMeals = result.MealPlans
            .Where(mealPlan => mealPlan.Meals.Any())
            .OrderBy(mealPlan => mealPlan.Date)
            .Select(mealPlan => mealPlan.Date.Date)
            .FirstOrDefault();

        return firstDayWithMeals == default
            ? result.WeekStart.Date
            : firstDayWithMeals;
    }

    private static DateTime NormalizeSelectedDate(DateTime? selectedDate, DateTime weekStart)
    {
        var normalizedWeekStart = WeekDateHelper.GetWeekStart(weekStart);
        var normalizedSelectedDate = selectedDate?.Date ?? normalizedWeekStart;
        var weekEnd = normalizedWeekStart.AddDays(6);

        if (normalizedSelectedDate < normalizedWeekStart)
        {
            return normalizedWeekStart;
        }

        if (normalizedSelectedDate > weekEnd)
        {
            return weekEnd;
        }

        return normalizedSelectedDate;
    }

    private static WeeklyMealItemDto MapWeeklyMeal(Meal meal)
    {
        return new WeeklyMealItemDto
        {
            MealId = meal.Id,
            RecipeId = meal.RecipeId,
            MealType = meal.MealType.ToString(),
            RecipeName = meal.Recipe.Name,
            Calories = MealPlanMath.CalculateMealCalories(meal),
            PortionMultiplier = meal.PortionMultiplier,
            Nutrition = MapNutrition(MealPlanMath.CalculateMealNutrition(meal))
        };
    }

    private static MealPlanDto MapToDto(MealPlan mealPlan)
    {
        var mealItems = mealPlan.Meals
            .OrderBy(m => m.MealType)
            .Select(m => new MealPlanMealDto
            {
                MealId = m.Id,
                RecipeId = m.RecipeId,
                MealType = m.MealType.ToString(),
                RecipeName = m.Recipe?.Name ?? "Unknown",
                Calories = m.Recipe is null ? 0 : MealPlanMath.CalculateMealCalories(m),
                PortionMultiplier = m.PortionMultiplier,
                Nutrition = m.Recipe is null ? new NutritionSummaryDto() : MapNutrition(MealPlanMath.CalculateMealNutrition(m))
            })
            .ToList();

        var ingredientUsage = mealPlan.Meals
            .Where(m => m.Recipe is not null)
            .SelectMany(m => m.Recipe.RecipeIngredients.Select(ri => new
            {
                IngredientName = ri.Ingredient.Name,
                QuantityInGrams = MealPlanMath.CalculateIngredientQuantity(m, ri)
            }))
            .GroupBy(x => x.IngredientName)
            .Select(g => new MostUsedIngredientSummaryDto
            {
                Name = g.Key,
                UsageCount = g.Count(),
                TotalQuantityInGrams = g.Sum(x => x.QuantityInGrams)
            })
            .OrderByDescending(x => x.UsageCount)
            .ThenByDescending(x => x.TotalQuantityInGrams)
            .Take(5)
            .ToList();

        return new MealPlanDto
        {
            Id = mealPlan.Id,
            Date = mealPlan.Date,
            MealsCount = mealItems.Count,
            TotalCalories = mealItems.Sum(m => m.Calories),
            TotalNutrition = new NutritionSummaryDto
            {
                Calories = mealItems.Sum(meal => meal.Calories),
                ProteinGrams = Math.Round(mealItems.Sum(meal => meal.Nutrition.ProteinGrams), 1, MidpointRounding.AwayFromZero),
                CarbsGrams = Math.Round(mealItems.Sum(meal => meal.Nutrition.CarbsGrams), 1, MidpointRounding.AwayFromZero),
                FatGrams = Math.Round(mealItems.Sum(meal => meal.Nutrition.FatGrams), 1, MidpointRounding.AwayFromZero)
            },
            Meals = mealItems,
            MostUsedIngredients = ingredientUsage
        };
    }

    private static MealPlan MapToEntity(MealPlanDto dto)
    {
        return new MealPlan
        {
            Id = dto.Id,
            Date = dto.Date
        };
    }

    private static GeneratePersonalizedMealPlanDto BuildDefaultGeneratePlan(PlannerPreferencesResult preferences, DateTime weekStart)
    {
        return new GeneratePersonalizedMealPlanDto
        {
            WeekStart = weekStart,
            MealsPerDay = Math.Clamp(preferences.MealsPerDay, 1, 3),
            BodyWeightKg = preferences.BodyWeightKg,
            ProteinTargetGrams = preferences.ProteinTargetGrams,
            CarbsTargetGrams = preferences.CarbsTargetGrams,
            FatTargetGrams = preferences.FatTargetGrams,
            ExcludedFoods = preferences.ExcludedFoods,
            ExcludedIngredientIds = preferences.ExcludedIngredientIds.ToList(),
            AllergyIngredientIds = preferences.AllergyIngredientIds.ToList()
        };
    }

    private static IReadOnlyCollection<string> ParseExcludedFoods(string? excludedFoods)
    {
        return (excludedFoods ?? string.Empty)
            .Split([',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static NutritionSummaryDto MapNutrition(NutritionSummaryResult nutrition)
    {
        return new NutritionSummaryDto
        {
            Calories = nutrition.Calories,
            ProteinGrams = nutrition.ProteinGrams,
            CarbsGrams = nutrition.CarbsGrams,
            FatGrams = nutrition.FatGrams
        };
    }
}
