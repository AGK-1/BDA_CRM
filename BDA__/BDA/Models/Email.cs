namespace BDA.Models
{
	public class Email
	{
		public int Id { get; set; }
		public string Subject { get; set; }
		public DateTime SentAt { get; set; }
		public bool IsResponse { get; set; }

		// Foreign Keys
		public int SentByUserId { get; set; }
		public User SentByUser { get; set; } // Many Emails are sent by one User

		public int SentToCustomerId { get; set; }
		public Customers SentToCustomer { get; set; } // Many Emails are sent to one Customer
	}
}
