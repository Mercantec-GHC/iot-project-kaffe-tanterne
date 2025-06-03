using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using KaffeMaskineProjekt.Repository;
using OpenTelemetry.Trace;
using KaffeMaskineProjekt.DomainModels;

namespace KaffeMaskineProjekt.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KaffeDBContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(KaffeDBContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(KaffeDBContext dbContext, CancellationToken cancellationToken)
    {
        List<Ingredient> ingredients = [new Ingredient { Name = "Instant Coffee" },
            new Ingredient { Name = "Sugar" },
            new Ingredient { Name = "Water" }];

        List<User> users = [
            new User { 
                Name = "user", 
                Password = "Password1234", 
                Email = "basic@basic.com" },
            new User {
                Name = "admin", 
                Password = "Password1234", 
                Email = "admin@adminstuff.com",
                Roles = new List<string> { "Admin" }
            }
        ];

        List<Recipe> recipes = [
            new()
            {
                Name = "Frappe",
                IngredientRecipes = new List<RecipeIngredient>()
                {
                    new RecipeIngredient()
                    {
                        Ingredient = ingredients[0],
                        Amount = 25
                    },
                    new RecipeIngredient()
                    {
                        Ingredient = ingredients[1],
                        Amount = 10
                    },
                    new RecipeIngredient()
                    {
                        Ingredient = ingredients[2],
                        Amount = 2000
                    }
                }
            },
            new()
            {
                Name = "Espresso",
                IngredientRecipes = new List<RecipeIngredient>()
                {
                    new RecipeIngredient()
                    {
                        Ingredient = ingredients[0],
                        Amount = 10
                    },
                    new RecipeIngredient()
                    {
                        Ingredient = ingredients[1],
                        Amount = 5
                    },
                    new RecipeIngredient()
                    {
                        Ingredient = ingredients[2],
                        Amount = 1000
                    }
                }
            }
        ];



        Order order = new()
        {
            Recipe = recipes[0],
            User = users[1],
            HasBeenServed = OrderStatus.Served,
        };

        // Create some measurements for the first and last ingredients and make variations over the course of the last day
        List<Measurements> measurements = new();


        // We need to create a bunch over the course of the last day. Simulating a lot of measurements.
        // We only do this for the first ingredient and the last ingredient, as the middle one is not used in any recipe.
        var now = DateTime.UtcNow;
        var firstIngredient = ingredients[0];
        var lastIngredient = ingredients[2];

        var firstIngredientTime = now.AddHours(-1); // Start 24 hours ago
        var lastIngredientTime = now.AddHours(-1); // Start 24 hours ago

        while (firstIngredientTime < now)
        {
            // Add measurements for the first ingredient
            measurements.Add(new Measurements
            {
                Ingredient = firstIngredient,
                Time = firstIngredientTime,
                Value = Random.Shared.Next(0, 50)
            });
            
            firstIngredientTime = firstIngredientTime.AddMinutes(30); // Increase time by 30 minutes
            // Add measurements for the last ingredient
            measurements.Add(new Measurements
            {
                Ingredient = lastIngredient,
                Time = lastIngredientTime,
                Value = Random.Shared.Next(0, 1600)
            });
            lastIngredientTime = lastIngredientTime.AddMinutes(30); // Increase time by 30 minutes
        }

        List<Statistics> statistics = [
            new()
            {
                NumberOfUses = 6,
                Recipe = recipes[0],
                User = users[1]
            },
            new()
            {
                NumberOfUses = 3,
                Recipe = recipes[1],
                User = users[0]
            }
        ];

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Ingredients.AddRangeAsync(ingredients, cancellationToken);
            await dbContext.Users.AddRangeAsync(users, cancellationToken);
            await dbContext.Recipes.AddRangeAsync(recipes, cancellationToken);
            await dbContext.Orders.AddAsync(order, cancellationToken);
            await dbContext.Measurements.AddRangeAsync(measurements, cancellationToken);
            await dbContext.Statistics.AddRangeAsync(statistics, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });




        /*SupportTicket firstTicket = new()
        {
            Title = "Test Ticket",
            Description = "Default ticket, please ignore!",
            Completed = true
        };

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Tickets.AddAsync(firstTicket, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });*/
    }
}