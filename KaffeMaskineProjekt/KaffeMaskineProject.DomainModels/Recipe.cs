namespace KaffeMaskineProject.DomainModels
{
	public class Recipe
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public virtual ICollection<RecipeIngredient> IngredientRecipes { get; set; }
    }
}
