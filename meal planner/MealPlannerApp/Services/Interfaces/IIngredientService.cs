using MealPlannerApp.Models;

namespace MealPlannerApp.Services.Interfaces;

public interface IIngredientService
{
    Task<IEnumerable<Ingredient>> GetAllIngredients();
    Task<Ingredient?> GetIngredientById(int id);
    Task<Ingredient> CreateIngredient(Ingredient ingredient);
    Task<bool> UpdateIngredient(Ingredient ingredient);
    Task<bool> DeleteIngredient(int id);
}
