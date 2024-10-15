using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BDA.Models
{
	public class Customers
	{
		//[StringLength(30, ErrorMessage = "Add the name please")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		[StringLength(30, ErrorMessage = "Add the name please")]
		public string Name { get; set; }
		[Required]
		[StringLength(30, ErrorMessage = "Add the surname please")]
		public string Surname { get; set; }
		[Required(ErrorMessage = "Please enter a phone number")]
		[RegularExpression(@"^\d{9}$", ErrorMessage = "Phone number must be exactly 9 digits.")]
		public string PhoneNumber { get; set; }

		[DataType(DataType.EmailAddress)]
		[Required(ErrorMessage = "Please enter Email ID")]
		[EmailAddress(ErrorMessage = "Please enter a valid email address")]
		public string Email { get; set; }

		//[Required(ErrorMessage = "Please enter company name")]
		public string? Company { get; set; }
		public string? Department { get; set; }
		public string? Position { get; set; }
		public DateTime CreatedAt { get; set; }

		// Foreign Key
		public int? CreatedByUserId { get; set; }
		public User? CreatedByUser { get; set; } // One-to-Many (Many customers are created by one user)

		public ICollection<Lead>? Leads { get; set; } = new List<Lead>(); // One-to-Many (A customer has many leads)
		public ICollection<Email>? ReceivedEmails { get; set; } = new List<Email>();
	
	}
}
