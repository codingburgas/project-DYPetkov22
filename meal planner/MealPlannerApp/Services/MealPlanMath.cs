using MealPlannerApp.Models;
using MealPlannerApp.Services.Models;

namespace MealPlannerApp.Services;

public static class MealPlanMath
{
    private static readonly IReadOnlyDictionary<int, double[]> MealCalorieSharesByMealCount =
        new Dictionary<int, double[]>
        {
            [1] = [1.0],
            [2] = [0.45, 0.55],
            [3] = [0.25, 0.35, 0.40]
        };

    public static int CalculateMealCalories(Meal meal)
    {
        return (int)Math.Round(meal.Recipe.Calories * meal.PortionMultiplier, MidpointRounding.AwayFromZero);
    }

    public static int CalculateIngredientQuantity(Meal meal, RecipeIngredient recipeIngredient)
    {
        return (int)Math.Round(recipeIngredient.QuantityInGrams * meal.PortionMultiplier, MidpointRounding.AwayFromZero);
    }

    public static NutritionSummaryResult CalculateRecipeNutrition(Recipe recipe)
    {
        var nutrition = new NutritionSummaryResult
        {
            Calories = recipe.Calories
        };

        foreach (var recipeIngredient in recipe.RecipeIngredients)
        {
            var multiplier = recipeIngredient.QuantityInGrams / 100.0;
            nutrition.ProteinGrams += recipeIngredient.Ingredient.ProteinPer100g * multiplier;
            nutrition.CarbsGrams += recipeIngredient.Ingredient.CarbsPer100g * multiplier;
            nutrition.FatGrams += recipeIngredient.Ingredient.FatPer100g * multiplier;
        }

        nutrition.ProteinGrams = RoundNutritionValue(nutrition.ProteinGrams);
        nutrition.CarbsGrams = RoundNutritionValue(nutrition.CarbsGrams);
        nutrition.FatGrams = RoundNutritionValue(nutrition.FatGrams);
        return nutrition;
    }

    public static NutritionSummaryResult CalculateMealNutrition(Meal meal)
    {
        return ScaleNutrition(CalculateRecipeNutrition(meal.Recipe), meal.PortionMultiplier);
    }

    public static double CalculatePortionMultiplier(double bodyWeightKg)
    {
        return Math.Round(bodyWeightKg / 70.0, 2, MidpointRounding.AwayFromZero);
    }

    public static int CalculateDailyCalorieTarget(double bodyWeightKg)
    {
        var normalizedWeight = Math.Clamp(bodyWeightKg, 40.0, 180.0);
        return (int)Math.Round(normalizedWeight * 30.0, MidpointRounding.AwayFromZero);
    }

    public static IReadOnlyList<double> GetMealCalorieShares(int mealsPerDay)
    {
        return MealCalorieSharesByMealCount.TryGetValue(mealsPerDay, out var shares)
            ? shares
            : MealCalorieSharesByMealCount[3];
    }

    public static int CalculateMealTargetCalories(int dailyCaloriesTarget, double share)
    {
        return (int)Math.Round(dailyCaloriesTarget * share, MidpointRounding.AwayFromZero);
    }

    public static NutritionSummaryResult CalculateDefaultDailyMacroTargets(double bodyWeightKg)
    {
        var normalizedWeight = Math.Clamp(bodyWeightKg, 40.0, 180.0);
        var calories = CalculateDailyCalorieTarget(normalizedWeight);
        var protein = normalizedWeight * 1.8;
        var fat = normalizedWeight * 0.8;
        var carbs = Math.Max((calories - (protein * 4) - (fat * 9)) / 4.0, 20.0);

        return new NutritionSummaryResult
        {
            Calories = calories,
            ProteinGrams = RoundNutritionValue(protein),
            CarbsGrams = RoundNutritionValue(carbs),
            FatGrams = RoundNutritionValue(fat)
        };
    }

    public static int CalculateDailyCaloriesFromMacros(double proteinGrams, double carbsGrams, double fatGrams)
    {
        return (int)Math.Round((proteinGrams * 4) + (carbsGrams * 4) + (fatGrams * 9), MidpointRounding.AwayFromZero);
    }

    public static NutritionSummaryResult CalculateMealNutritionTarget(NutritionSummaryResult dailyTarget, double share)
    {
        return new NutritionSummaryResult
        {
            Calories = CalculateMealTargetCalories(dailyTarget.Calories, share),
            ProteinGrams = RoundNutritionValue(dailyTarget.ProteinGrams * share),
            CarbsGrams = RoundNutritionValue(dailyTarget.CarbsGrams * share),
            FatGrams = RoundNutritionValue(dailyTarget.FatGrams * share)
        };
    }

    public static NutritionSummaryResult ScaleNutrition(NutritionSummaryResult nutrition, double multiplier)
    {
        return new NutritionSummaryResult
        {
            Calories = (int)Math.Round(nutrition.Calories * multiplier, MidpointRounding.AwayFromZero),
            ProteinGrams = RoundNutritionValue(nutrition.ProteinGrams * multiplier),
            CarbsGrams = RoundNutritionValue(nutrition.CarbsGrams * multiplier),
            FatGrams = RoundNutritionValue(nutrition.FatGrams * multiplier)
        };
    }

    public static double CalculatePortionMultiplierForCalories(int targetCalories, int recipeCalories)
    {
        if (recipeCalories <= 0)
        {
            return 1.0;
        }

        var multiplier = (double)targetCalories / recipeCalories;
        return Math.Round(Math.Clamp(multiplier, 0.5, 3.0), 1, MidpointRounding.AwayFromZero);
    }

    public static double CalculatePortionMultiplierForNutrition(NutritionSummaryResult targetNutrition, NutritionSummaryResult recipeNutrition)
    {
        var ratios = new List<double>();

        if (recipeNutrition.Calories > 0 && targetNutrition.Calories > 0)
        {
            ratios.Add((double)targetNutrition.Calories / recipeNutrition.Calories);
        }

        if (recipeNutrition.ProteinGrams > 0 && targetNutrition.ProteinGrams > 0)
        {
            ratios.Add(targetNutrition.ProteinGrams / recipeNutrition.ProteinGrams);
        }

        if (recipeNutrition.CarbsGrams > 0 && targetNutrition.CarbsGrams > 0)
        {
            ratios.Add(targetNutrition.CarbsGrams / recipeNutrition.CarbsGrams);
        }

        if (recipeNutrition.FatGrams > 0 && targetNutrition.FatGrams > 0)
        {
            ratios.Add(targetNutrition.FatGrams / recipeNutrition.FatGrams);
        }

        if (ratios.Count == 0)
        {
            return 1.0;
        }

        return Math.Round(Math.Clamp(ratios.Average(), 0.5, 3.0), 1, MidpointRounding.AwayFromZero);
    }

    private static double RoundNutritionValue(double value)
    {
        return Math.Round(value, 1, MidpointRounding.AwayFromZero);
    }
}
