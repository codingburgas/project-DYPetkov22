using MealPlannerApp.Data;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using MealPlannerApp.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Services;

public class IngredientService : IIngredientService
{
    private readonly ApplicationDbContext _dbContext;

    public IngredientService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Ingredient>> GetAllIngredients()
    {
        return await _dbContext.Ingredients
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<Ingredient?> GetIngredientById(int id)
    {
        return await _dbContext.Ingredients
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Ingredient> CreateIngredient(Ingredient ingredient)
    {
        ingredient.CreatedAt = DateTime.UtcNow;
        _dbContext.Ingredients.Add(ingredient);
        await _dbContext.SaveChangesAsync();
        return ingredient;
    }

    public async Task<bool> UpdateIngredient(Ingredient ingredient)
    {
        var existingIngredient = await _dbContext.Ingredients.FindAsync(ingredient.Id);
        if (existingIngredient is null)
        {
            return false;
        }

        existingIngredient.Name = ingredient.Name;
        existingIngredient.CaloriesPer100g = ingredient.CaloriesPer100g;
        existingIngredient.ProteinPer100g = ingredient.ProteinPer100g;
        existingIngredient.CarbsPer100g = ingredient.CarbsPer100g;
        existingIngredient.FatPer100g = ingredient.FatPer100g;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<DeleteOperationResult> DeleteIngredient(int id)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync(id);
        if (ingredient is null)
        {
            return DeleteOperationResult.NotFound;
        }

        var isInUse = await _dbContext.RecipeIngredients
            .AnyAsync(ri => ri.IngredientId == id)
            || await _dbContext.UserIngredientPreferences.AnyAsync(preference => preference.IngredientId == id);
        if (isInUse)
        {
            return DeleteOperationResult.InUse;
        }

        _dbContext.Ingredients.Remove(ingredient);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return DeleteOperationResult.InUse;
        }

        return DeleteOperationResult.Deleted;
    }
}
