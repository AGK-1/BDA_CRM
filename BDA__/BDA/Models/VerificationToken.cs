namespace BDA.Models
{
    public class VerificationToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string UserkoId { get; set; } // Foreign key to User table
        public DateTime ExpiryDate { get; set; }

        public Userko Userko { get; set; } // Navigation property
    }
}
