using MealPlannerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MealPlannerApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<Meal> Meals => Set<Meal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.MealPlans)
            .WithOne(mp => mp.User)
            .HasForeignKey(mp => mp.UserId);

        modelBuilder.Entity<MealPlan>()
            .HasMany(mp => mp.Meals)
            .WithOne(m => m.MealPlan)
            .HasForeignKey(m => m.MealPlanId);

        modelBuilder.Entity<Meal>()
            .HasOne(m => m.Recipe)
            .WithMany(r => r.Meals)
            .HasForeignKey(m => m.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.RecipeIngredients)
            .HasForeignKey(ri => ri.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany(i => i.RecipeIngredients)
            .HasForeignKey(ri => ri.IngredientId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasIndex(ri => new { ri.RecipeId, ri.IngredientId })
            .IsUnique();

        var seedCreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                CreatedAt = seedCreatedAt,
                Username = "admin",
                Email = "admin@mealplanner.local",
                Role = UserRole.Admin
            });

        modelBuilder.Entity<Ingredient>().HasData(
            new Ingredient { Id = 1, CreatedAt = seedCreatedAt, Name = "Chicken Breast", CaloriesPer100g = 165 },
            new Ingredient { Id = 2, CreatedAt = seedCreatedAt, Name = "Rice", CaloriesPer100g = 130 },
            new Ingredient { Id = 3, CreatedAt = seedCreatedAt, Name = "Broccoli", CaloriesPer100g = 35 });

        modelBuilder.Entity<Recipe>().HasData(
            new Recipe
            {
                Id = 1,
                CreatedAt = seedCreatedAt,
                Name = "Grilled Chicken and Rice",
                Description = "Simple grilled chicken served with rice.",
                CookingTime = 35,
                Calories = 520,
                IsVegetarian = false
            },
            new Recipe
            {
                Id = 2,
                CreatedAt = seedCreatedAt,
                Name = "Broccoli Rice Bowl",
                Description = "Steamed broccoli with rice.",
                CookingTime = 20,
                Calories = 340,
                IsVegetarian = true
            });

        modelBuilder.Entity<RecipeIngredient>().HasData(
            new RecipeIngredient { Id = 1, CreatedAt = seedCreatedAt, RecipeId = 1, IngredientId = 1, QuantityInGrams = 200 },
            new RecipeIngredient { Id = 2, CreatedAt = seedCreatedAt, RecipeId = 1, IngredientId = 2, QuantityInGrams = 150 },
            new RecipeIngredient { Id = 3, CreatedAt = seedCreatedAt, RecipeId = 2, IngredientId = 2, QuantityInGrams = 150 },
            new RecipeIngredient { Id = 4, CreatedAt = seedCreatedAt, RecipeId = 2, IngredientId = 3, QuantityInGrams = 180 });
    }
}
