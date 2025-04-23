using KaffeMaskineProject.DomainModels.Recipe;
using KaffeMaskineProject.DomainModels.Ingredient;

namespace KaffeMaskineProject.DomainModels
{
    public class IngredientRecipe
    {
        public int Id { get; set; }
        public Ingredient ingredient { get; set; } 
        public Recipe recipeId { get; set; } 
        public int amount { get; set; } 
    }
}
