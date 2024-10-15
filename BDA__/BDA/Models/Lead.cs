namespace BDA.Models
{
	public class Lead
	{
		public int Id { get; set; }
		public string Product { get; set; }
		public decimal ExpectedRevenue { get; set; }
		public decimal Probability { get; set; }
		public DateTime ExpectedClosingDate { get; set; }
		public string CurrentStage { get; set; }
		public string LostReason { get; set; }
		public DateTime LastEmailSentAt { get; set; }
		public DateTime LastClientResponseAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }

		// Foreign Keys
		public int CustomerId { get; set; }
		public Customers Customers { get; set; } // One-to-Many (One customer has many leads)

		public int? AgentId { get; set; }
		public User Agent { get; set; } // One-to-Many (One agent is responsible for many leads)

		public ICollection<LeadStageHistory> LeadStageHistories { get; set; } // One-to-Many (Lead has many stage histories)
	}

}
