using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaffeMaskineProjekt.DTO
{
    public class MeasurementsDTO
    {
        public int Id { get; set; }
        public DateTime Time { get; set; } = DateTime.Now.ToUniversalTime();
        public required int Value { get; set; }
        public required int IngredientId { get; set; }
        public IngredientDTO Ingredient { get; set; }
    }

    public class CreateMeasurementsModel
    {
        public required int Value { get; set; }
        public required int IngredientId { get; set; }
    }

    public class EditMeasurementsModel
    {
        public required int Id { get; set; }
        public required int Value { get; set; }
        public required int IngredientId { get; set; }
    }

}
