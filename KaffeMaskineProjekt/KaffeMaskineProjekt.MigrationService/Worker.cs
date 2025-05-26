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
            HasBeenServed = true,
        };

        Measurements measurements = new()
        {
            Ingredient = ingredients[0],
            Time = DateTime.Now.ToUniversalTime(),
            Value = 5
        };

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
            await dbContext.Measurements.AddAsync(measurements, cancellationToken);
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