using KaffeMaskineProject.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace KaffeMaskineProject.Repository
{
    public class KaffeDBContext : DbContext
    {
        public KaffeDBContext(DbContextOptions<KaffeDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Statistics> Statistics { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Measurements> Measurements { get; set; }


    }
}
