namespace KaffeMaskineProjekt.DomainModels
{
    public class Statistics
    {
        public int Id { get; set; }
        public Recipe Recipe { get; set; } 
        public User User { get; set; }
        public int NumberOfUses { get; set; }

    }
}

