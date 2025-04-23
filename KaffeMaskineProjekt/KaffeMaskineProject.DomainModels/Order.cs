using KaffeMaskineProject.DomainModels.User;
using KaffeMaskineProject.DomainModels.Recipe;

namespace KaffeMaskineProject.DomainModels
{
	public class Order
	{
		public int orderNumber { get; set; }
		public bool hasBeenServed { get; set; }
		public User userId { get; set; } 
		public Recipe recipeId { get; set; }
	}
}
