namespace KaffeMaskineProjekt.DomainModels
{
    public class Measurements
    {
        public int Id { get; set; }
        public DateTime Time { get; set; } = DateTime.Now.ToUniversalTime();
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
        public int Value { get; set; }
        //public string UnitOfMeasurement { get; set; }
    }
}
