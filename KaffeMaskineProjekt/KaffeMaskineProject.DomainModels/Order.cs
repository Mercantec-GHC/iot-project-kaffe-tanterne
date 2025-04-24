namespace KaffeMaskineProject.DomainModels
{
	public class Order
	{
		public int Id{ get; set; }
		public bool HasBeenServed { get; set; }
		public User User { get; set; } 
		public Recipe Recipe { get; set; }
	}
}
