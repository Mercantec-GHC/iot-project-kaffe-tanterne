namespace KaffeMaskineProjekt.DomainModels
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
    }
}