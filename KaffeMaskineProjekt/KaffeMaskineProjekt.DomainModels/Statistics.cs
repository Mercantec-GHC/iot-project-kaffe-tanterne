namespace KaffeMaskineProjekt.DomainModels
{
    public class Statistics
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int NumberOfUses { get; set; }

    }
}

