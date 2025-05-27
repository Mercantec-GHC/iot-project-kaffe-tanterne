namespace KaffeMaskineProjekt.DomainModels
{
	public class Order
	{
		public int Id{ get; set; }
		public bool HasBeenServed { get; set; }
		public int UserId { get; set; }
		public int RecipeId { get; set; }
		public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public User User { get; set; } 
		public Recipe Recipe { get; set; }
	}
}
