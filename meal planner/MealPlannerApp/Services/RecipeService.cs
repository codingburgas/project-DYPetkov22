using MealPlannerApp.Data;
using MealPlannerApp.Models;
using MealPlannerApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Services;

public class RecipeService : IRecipeService
{
    private readonly ApplicationDbContext _dbContext;

    public RecipeService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Recipe>> GetAllRecipes(string? ingredientName = null, bool vegetarianOnly = false, bool highProteinOnly = false)
    {
        IQueryable<Recipe> query = _dbContext.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient);

        if (!string.IsNullOrWhiteSpace(ingredientName))
        {
            query = query.Where(r =>
                r.RecipeIngredients.Any(ri =>
                    ri.Ingredient.Name.Contains(ingredientName)));
        }

        if (vegetarianOnly)
        {
            query = query.Where(r => r.IsVegetarian);
        }

        if (highProteinOnly)
        {
            var proteinKeywords = new[] { "chicken", "beef", "turkey", "egg", "tuna", "salmon", "tofu", "lentil", "beans", "yogurt" };
            query = query.Where(r =>
                r.RecipeIngredients.Any(ri =>
                    proteinKeywords.Any(k =>
                        ri.Ingredient.Name.ToLower().Contains(k))));
        }

        return await query.ToListAsync();
    }

    public async Task<Recipe?> GetRecipeById(int id)
    {
        return await _dbContext.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Recipe> CreateRecipe(Recipe recipe)
    {
        recipe.CreatedAt = DateTime.UtcNow;
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();
        return recipe;
    }

    public async Task<bool> UpdateRecipe(Recipe recipe)
    {
        var existingRecipe = await _dbContext.Recipes.FindAsync(recipe.Id);
        if (existingRecipe is null)
        {
            return false;
        }

        existingRecipe.Name = recipe.Name;
        existingRecipe.Description = recipe.Description;
        existingRecipe.CookingTime = recipe.CookingTime;
        existingRecipe.Calories = recipe.Calories;
        existingRecipe.IsVegetarian = recipe.IsVegetarian;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRecipe(int id)
    {
        var recipe = await _dbContext.Recipes.FindAsync(id);
        if (recipe is null)
        {
            return false;
        }

        _dbContext.Recipes.Remove(recipe);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
