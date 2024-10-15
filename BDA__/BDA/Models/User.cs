using System.Text.Json.Serialization;
using OfficeOpenXml.Style;

namespace BDA.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Surname { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Company_name { get; set; }
        public string Company_domain { get; set; }
		public string Role { get; set; }
		public DateTime CreatedAt { get; set; }

		// Relationships
		public ICollection<Customers> Customers { get; set; } // One-to-Many with Customers
		public ICollection<Email> SentEmails { get; set; }   // One-to-Many with Emails
		public ICollection<LeadStageHistory> LeadStageHistories { get; set; } // One-to-Many with LeadStageHistory

		// Missing Leads collection, add it here
		public ICollection<Lead> Leads { get; set; } // One-to-Many (User as Agent for Leads)
	}
}
