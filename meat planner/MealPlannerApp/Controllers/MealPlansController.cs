using MealPlannerApp.Dtos.MealPlans;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MealPlannerApp.Controllers;

public class MealPlansController : Controller
{
    private readonly IMealPlanService _mealPlanService;
    private readonly IRecipeService _recipeService;

    public MealPlansController(IMealPlanService mealPlanService, IRecipeService recipeService)
    {
        _mealPlanService = mealPlanService;
        _recipeService = recipeService;
    }

    public async Task<IActionResult> Index()
    {
        var mealPlans = await _mealPlanService.GetAllMealPlans();
        var dto = mealPlans.Select(MapToDto).ToList();
        return View(dto);
    }

    public async Task<IActionResult> Details(int id)
    {
        var mealPlan = await _mealPlanService.GetMealPlanById(id);
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

        await _mealPlanService.CreateMealPlan(MapToEntity(dto));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var mealPlan = await _mealPlanService.GetMealPlanById(id);
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

        var updated = await _mealPlanService.UpdateMealPlan(MapToEntity(dto));
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var mealPlan = await _mealPlanService.GetMealPlanById(id);
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
        var deleted = await _mealPlanService.DeleteMealPlan(id);
        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Weekly(int userId = 1, DateTime? weekStart = null)
    {
        var result = await _mealPlanService.GetWeeklyPlan(userId, weekStart);
        var model = BuildWeeklyPlannerDto(result, userId);

        ViewData["UserId"] = userId;
        ViewData["WeekStart"] = result.WeekStart.ToString("yyyy-MM-dd");
        return View(model);
    }

    public async Task<IActionResult> AddMeal(int userId = 1, DateTime? weekStart = null)
    {
        var result = await _mealPlanService.GetWeeklyPlan(userId, weekStart);
        if (!result.MealPlans.Any())
        {
            for (var offset = 0; offset < 7; offset++)
            {
                await _mealPlanService.CreateMealPlan(new MealPlan
                {
                    UserId = userId,
                    Date = result.WeekStart.AddDays(offset)
                });
            }

            result = await _mealPlanService.GetWeeklyPlan(userId, result.WeekStart);
        }

        var model = new AddMealDto
        {
            UserId = userId,
            WeekStart = result.WeekStart
        };

        await PopulateAddMealLookups(userId, result.WeekStart);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMeal(AddMealDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAddMealLookups(dto.UserId, dto.WeekStart);
            return View(dto);
        }

        await _mealPlanService.AddMealToPlan(dto.MealPlanId, new Meal
        {
            RecipeId = dto.RecipeId,
            MealType = dto.MealType
        });

        return RedirectToAction(nameof(Weekly), new { userId = dto.UserId, weekStart = dto.WeekStart.ToString("yyyy-MM-dd") });
    }

    private async Task PopulateAddMealLookups(int userId, DateTime weekStart)
    {
        var weeklyPlan = await _mealPlanService.GetWeeklyPlan(userId, weekStart);
        var recipes = await _recipeService.GetAllRecipes();

        ViewBag.MealPlans = weeklyPlan.MealPlans
            .OrderBy(mp => mp.Date)
            .Select(mp => new SelectListItem
            {
                Value = mp.Id.ToString(),
                Text = $"{mp.Date:ddd, dd MMM yyyy}"
            })
            .ToList();

        ViewBag.Recipes = recipes
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.Name} ({r.Calories} kcal)"
            })
            .ToList();
    }

    private static WeeklyMealPlannerDto BuildWeeklyPlannerDto(Services.Models.WeeklyMealPlanResult result, int userId)
    {
        var dayMap = result.MealPlans.ToDictionary(mp => mp.Date.Date, mp => mp);
        var days = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = result.WeekStart.AddDays(offset).Date;
                var meals = dayMap.TryGetValue(date, out var mealPlan)
                    ? mealPlan.Meals
                        .OrderBy(m => m.MealType)
                        .Select(m => new WeeklyMealItemDto
                        {
                            MealType = m.MealType.ToString(),
                            RecipeName = m.Recipe.Name,
                            Calories = m.Recipe.Calories
                        })
                        .ToList()
                    : new List<WeeklyMealItemDto>();

                return new WeeklyDayDto
                {
                    Date = date,
                    TotalCalories = meals.Sum(m => m.Calories),
                    Meals = meals
                };
            })
            .ToList();

        return new WeeklyMealPlannerDto
        {
            UserId = userId,
            WeekStart = result.WeekStart,
            WeekEnd = result.WeekEnd,
            WeeklyTotalCalories = result.WeeklyTotalCalories,
            Days = days,
            MostUsedIngredients = result.MostUsedIngredients
                .Select(i => new MostUsedIngredientDto
                {
                    Name = i.Name,
                    UsageCount = i.UsageCount,
                    TotalQuantityInGrams = i.TotalQuantityInGrams
                })
                .ToList()
        };
    }

    private static MealPlanDto MapToDto(MealPlan mealPlan)
    {
        var mealItems = mealPlan.Meals
            .OrderBy(m => m.MealType)
            .Select(m => new MealPlanMealDto
            {
                MealType = m.MealType.ToString(),
                RecipeName = m.Recipe?.Name ?? "Unknown",
                Calories = m.Recipe?.Calories ?? 0
            })
            .ToList();

        var ingredientUsage = mealPlan.Meals
            .SelectMany(m => m.Recipe?.RecipeIngredients ?? [])
            .GroupBy(ri => ri.Ingredient.Name)
            .Select(g => new MostUsedIngredientSummaryDto
            {
                Name = g.Key,
                UsageCount = g.Count()
            })
            .OrderByDescending(x => x.UsageCount)
            .Take(5)
            .ToList();

        return new MealPlanDto
        {
            Id = mealPlan.Id,
            UserId = mealPlan.UserId,
            Date = mealPlan.Date,
            MealsCount = mealItems.Count,
            TotalCalories = mealItems.Sum(m => m.Calories),
            Meals = mealItems,
            MostUsedIngredients = ingredientUsage
        };
    }

    private static MealPlan MapToEntity(MealPlanDto dto)
    {
        return new MealPlan
        {
            Id = dto.Id,
            UserId = dto.UserId,
            Date = dto.Date
        };
    }
}
