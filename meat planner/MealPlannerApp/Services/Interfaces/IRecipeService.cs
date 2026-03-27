using MealPlannerApp.Models;

namespace MealPlannerApp.Services.Interfaces;

public interface IRecipeService
{
    Task<IEnumerable<Recipe>> GetAllRecipes(string? ingredientName = null, bool vegetarianOnly = false, bool highProteinOnly = false);
    Task<Recipe?> GetRecipeById(int id);
    Task<Recipe> CreateRecipe(Recipe recipe);
    Task<bool> UpdateRecipe(Recipe recipe);
    Task<bool> DeleteRecipe(int id);
}
