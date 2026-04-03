using MealPlannerApp.Dtos.Ingredients;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MealPlannerApp.Controllers;

public class IngredientsController : Controller
{
    private readonly IIngredientService _ingredientService;

    public IngredientsController(IIngredientService ingredientService)
    {
        _ingredientService = ingredientService;
    }

    public async Task<IActionResult> Index()
    {
        var ingredients = await _ingredientService.GetAllIngredients();
        var dto = ingredients.Select(MapToDto).ToList();
        return View(dto);
    }

    public async Task<IActionResult> Details(int id)
    {
        var ingredient = await _ingredientService.GetIngredientById(id);
        if (ingredient is null)
        {
            return NotFound();
        }

        return View(MapToDto(ingredient));
    }

    public IActionResult Create()
    {
        return View(new IngredientDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IngredientDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await _ingredientService.CreateIngredient(MapToEntity(dto));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var ingredient = await _ingredientService.GetIngredientById(id);
        if (ingredient is null)
        {
            return NotFound();
        }

        return View(MapToDto(ingredient));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, IngredientDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var updated = await _ingredientService.UpdateIngredient(MapToEntity(dto));
        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var ingredient = await _ingredientService.GetIngredientById(id);
        if (ingredient is null)
        {
            return NotFound();
        }

        return View(MapToDto(ingredient));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _ingredientService.DeleteIngredient(id);
        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private static IngredientDto MapToDto(Ingredient ingredient)
    {
        return new IngredientDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            CaloriesPer100g = ingredient.CaloriesPer100g
        };
    }

    private static Ingredient MapToEntity(IngredientDto dto)
    {
        return new Ingredient
        {
            Id = dto.Id,
            Name = dto.Name,
            CaloriesPer100g = dto.CaloriesPer100g
        };
    }
}
