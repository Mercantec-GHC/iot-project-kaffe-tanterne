using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaffeMaskineProjekt.DTO
{
    public class StatisticsDTO
    {
        public required int Id { get; set; }
        public required int RecipeId { get; set; }
        public required int UserId { get; set; }
        public required int NumberOfUses { get; set; }
    }

    public class CreateStatisticsModel
    {
        public required int RecipeId { get; set; }
        public required int UserId { get; set; }
        public required int NumberOfUses { get; set; }
    }

    public class EditStatisticsModel
    {
        public required int Id { get; set; }
        public required int RecipeId { get; set; }
        public required int UserId { get; set; }
        public required int NumberOfUses { get; set; }
    }
}
