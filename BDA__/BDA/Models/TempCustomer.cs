using System.ComponentModel.DataAnnotations;

namespace BDA.Models
{
	public class TempCustomer
	{
		public int Id { get; set; } // Consider using a different approach if you're not using IDs for temp storage
		[Required]
		public string? Name { get; set; }
		public string? Surname { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Email { get; set; }
		public string? Company { get; set; }
		public string? Department { get; set; }
		public string? Position { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current time
	}
}
