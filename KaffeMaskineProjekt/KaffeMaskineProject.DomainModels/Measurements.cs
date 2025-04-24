namespace KaffeMaskineProject.DomainModels
{
    public class Measurements
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public Ingredient Ingredient { get; set; }
        public int Value { get; set; }
    }
}
