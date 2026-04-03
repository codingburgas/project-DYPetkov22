using MealPlannerApp.Dtos.Recipes;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MealPlannerApp.Controllers;

public class RecipesController : Controller
{
    private readonly IRecipeService _recipeService;

    public RecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    public async Task<IActionResult> Index(string? ingredientName, bool vegetarianOnly = false, bool highProteinOnly = false)
    {
        var recipes = await _recipeService.GetAllRecipes(ingredientName, vegetarianOnly, highProteinOnly);
        var dto = recipes.Select(MapToDto).ToList();
        ViewData["IngredientName"] = ingredientName;
        ViewData["VegetarianOnly"] = vegetarianOnly;
        ViewData["HighProteinOnly"] = highProteinOnly;
        return View(dto);
    }

    public async Task<IActionResult> Details(int id)
    {
        var recipe = await _recipeService.GetRecipeById(id);
        if (recipe is null)
        {
            return NotFound();
        }

        return View(MapToDto(recipe));
    }

    public IActionResult Create()
    {
        return View(new RecipeDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RecipeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await _recipeService.CreateRecipe(MapToEntity(dto));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var recipe = await _recipeService.GetRecipeById(id);
        if (recipe is null)
        {
            return NotFound();
        }

        return View(MapToDto(recipe));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RecipeDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var updated = await _recipeService.UpdateRecipe(MapToEntity(dto));
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var recipe = await _recipeService.GetRecipeById(id);
        if (recipe is null)
        {
            return NotFound();
        }

        return View(MapToDto(recipe));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _recipeService.DeleteRecipe(id);
        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private static RecipeDto MapToDto(Recipe recipe)
    {
        return new RecipeDto
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            CookingTime = recipe.CookingTime,
            Calories = recipe.Calories,
            IsVegetarian = recipe.IsVegetarian
        };
    }

    private static Recipe MapToEntity(RecipeDto dto)
    {
        return new Recipe
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            CookingTime = dto.CookingTime,
            Calories = dto.Calories,
            IsVegetarian = dto.IsVegetarian
        };
    }
}
