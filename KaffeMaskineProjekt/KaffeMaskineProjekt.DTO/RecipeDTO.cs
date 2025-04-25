namespace KaffeMaskineProjekt.DTO
{
    public class RecipeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();
    }

    /// <summary>
    /// Model for creating a new recipe
    /// </summary>
    public class CreateRecipeModel
    {
        /// <summary>
        /// The name of the recipe
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Dictionary of ingredients with their amounts
        /// Key: Ingredient ID, Value: Amount
        /// </summary>
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();

    }

    /// <summary>
    /// Model for updating an existing recipe
    /// </summary>
    public class EditRecipeModel
    {
        /// <summary>
        /// The unique identifier of the recipe
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The updated name of the recipe
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Dictionary of ingredients with their amounts
        /// Key: Ingredient ID, Value: Amount
        /// </summary>
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();
    }

    /// <summary>
    /// Model for viewing recipe details
    /// </summary>
    public class ViewRecipeModel
    {
        /// <summary>
        /// The unique identifier of the recipe
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the recipe
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Dictionary of ingredients with their amounts
        /// Key: Ingredient ID, Value: Amount
        /// </summary>
        public Dictionary<int, int> Ingredients { get; set; } = new Dictionary<int, int>();
    }
}
