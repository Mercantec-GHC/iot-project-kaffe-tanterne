using KaffeMaskineProjekt.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaffeMaskineProjekt.DTO
{
    public class CreateOrderModel
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public OrderStatus HasBeenServed { get; set; }
    }
    public class EditOrderModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public OrderStatus HasBeenServed { get; set; }
    }
}
