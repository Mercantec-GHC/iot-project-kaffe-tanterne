namespace KaffeMaskineProject.DomainModels
{
    public class IngredientRecipe
    {
        public int Id { get; set; }
        public int IngredientId { get; set; }
        public int RecipeId { get; set; }
        public Ingredient Ingredient { get; set; } 
        public Recipe Recipe { get; set; } 
        public int Amount { get; set; } 
    }
}
