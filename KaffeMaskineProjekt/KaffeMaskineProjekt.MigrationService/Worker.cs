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
        Ingredient firstIngredient = new()
        {
            Name = "Instant Coffee"
        };
        Ingredient secondIngredient = new()
        {
            Name = "Sugar"
        };
        Ingredient thirdIngredient = new()
        {
            Name = "Water"
        };

        User defaultUser = new()
        {
            Name = "admin",
            Password = "Password1234",
        };

        Recipe recipe = new()
        {
            Name = "Frappe",
            IngredientRecipes = new List<RecipeIngredient>()
            {
                new RecipeIngredient()
                {
                    Ingredient = firstIngredient,
                    Amount = 25
                },
                new RecipeIngredient()
                {
                    Ingredient = secondIngredient,
                    Amount = 10
                },
                new RecipeIngredient()
                {
                    Ingredient = thirdIngredient,
                    Amount = 2000
                }
            }
        };

        Order order = new()
        {
            Recipe = recipe,
            User = defaultUser,
            HasBeenServed = false,
        };

        Measurements measurements = new()
        {
            Ingredient = firstIngredient,
            Time = DateTime.Now.ToUniversalTime(),
            Value = 5
        };

        Statistics statistics = new()
        {
            NumberOfUses = 0,
            Recipe = recipe,
            User = defaultUser
        };

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Ingredients.AddRangeAsync([firstIngredient, secondIngredient, thirdIngredient], cancellationToken);
            await dbContext.Users.AddAsync(defaultUser, cancellationToken);
            await dbContext.Recipes.AddAsync(recipe, cancellationToken);
            await dbContext.Orders.AddAsync(order, cancellationToken);
            await dbContext.Measurements.AddAsync(measurements, cancellationToken);
            await dbContext.Statistics.AddAsync(statistics, cancellationToken);
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