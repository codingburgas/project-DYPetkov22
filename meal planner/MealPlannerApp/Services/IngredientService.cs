using MealPlannerApp.Data;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
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
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteIngredient(int id)
    {
        var ingredient = await _dbContext.Ingredients.FindAsync(id);
        if (ingredient is null)
        {
            return false;
        }

        _dbContext.Ingredients.Remove(ingredient);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
