namespace KaffeMaskineProject.DomainModels
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<IngredientRecipe> IngredientRecipes { get; set; }
    }
}

