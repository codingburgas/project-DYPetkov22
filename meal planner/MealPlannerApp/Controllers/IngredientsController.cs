using MealPlannerApp.Dtos.Ingredients;
using MealPlannerApp.Infrastructure;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using MealPlannerApp.Services.Models;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize(Roles = ApplicationRoles.Admin)]
    public IActionResult Create()
    {
        return View(new IngredientDto());
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
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

    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var ingredient = await _ingredientService.GetIngredientById(id);
        if (ingredient is null)
        {
            return NotFound();
        }

        return View(MapToDto(ingredient));
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
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

    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var ingredient = await _ingredientService.GetIngredientById(id);
        if (ingredient is null)
        {
            return NotFound();
        }

        return View(MapToDto(ingredient));
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _ingredientService.DeleteIngredient(id);
        if (deleted == DeleteOperationResult.NotFound)
        {
            return NotFound();
        }

        if (deleted == DeleteOperationResult.InUse)
        {
            TempData["ErrorMessage"] = "This ingredient is still used by one or more recipes and cannot be deleted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

    private static IngredientDto MapToDto(Ingredient ingredient)
    {
        return new IngredientDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            CaloriesPer100g = ingredient.CaloriesPer100g,
            ProteinPer100g = ingredient.ProteinPer100g,
            CarbsPer100g = ingredient.CarbsPer100g,
            FatPer100g = ingredient.FatPer100g
        };
    }

    private static Ingredient MapToEntity(IngredientDto dto)
    {
        return new Ingredient
        {
            Id = dto.Id,
            Name = dto.Name,
            CaloriesPer100g = dto.CaloriesPer100g,
            ProteinPer100g = dto.ProteinPer100g,
            CarbsPer100g = dto.CarbsPer100g,
            FatPer100g = dto.FatPer100g
        };
    }
}
