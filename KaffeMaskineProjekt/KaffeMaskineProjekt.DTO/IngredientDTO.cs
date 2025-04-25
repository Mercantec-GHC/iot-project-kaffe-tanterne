namespace KaffeMaskineProjekt.DTO
{
    public class IngredientDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Model for creating a new ingredient
    /// </summary>
    public class CreateIngredientModel
    {
        /// <summary>
        /// The name of the ingredient
        /// </summary>
        public required string Name { get; set; }
    }

    /// <summary>
    /// Model for updating an existing ingredient
    /// </summary>
    public class EditIngredientModel
    {
        /// <summary>
        /// The unique identifier of the ingredient
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The updated name of the ingredient
        /// </summary>
        public required string Name { get; set; }
    }
}
