namespace BDA.Models
{
	public class LeadStageHistory
	{
		public int Id { get; set; }
		public string FromStage { get; set; }
		public string ToStage { get; set; }
		public string ChangedReason { get; set; }
		public DateTime ChangedAt { get; set; }

		// Foreign Keys
		public int LeadId { get; set; }
		public Lead Lead { get; set; } // Many-to-One (Many stage histories for one lead)

		public int ChangedByUserId { get; set; }
		public User ChangedByUser { get; set; } // Many-to-One (Stage changed by one user)
	}
}
