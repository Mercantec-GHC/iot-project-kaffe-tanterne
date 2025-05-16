namespace KaffeMaskineProjekt.DomainModels
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key relationship with User
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}