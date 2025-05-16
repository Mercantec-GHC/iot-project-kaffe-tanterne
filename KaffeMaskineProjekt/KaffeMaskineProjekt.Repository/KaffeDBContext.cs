using KaffeMaskineProjekt.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace KaffeMaskineProjekt.Repository
{
    public class KaffeDBContext : DbContext
    {
        public KaffeDBContext(DbContextOptions<KaffeDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure RefreshToken entity
            var entityType = modelBuilder.Entity<RefreshToken>();

            entityType.HasKey(rt => rt.Id); // Set Id as primary key
            entityType.Property(rt => rt.Token).HasMaxLength(200); // Token is required
            entityType.HasIndex(rt => rt.Token).IsUnique(); // Token should be unique
            entityType.HasOne(rt => rt.User) // Configure one-to-many relationship with User
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if user is deleted
        }

        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Statistics> Statistics { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Measurements> Measurements { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
