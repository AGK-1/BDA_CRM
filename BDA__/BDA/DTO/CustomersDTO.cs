using BDA.Models;

public class CustomerDTO
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Surname { get; set; }
	public string PhoneNumber { get; set; }
	public string Email { get; set; }
	public string Company { get; set; }
	public string Department { get; set; }
	public string Position { get; set; }
	public DateTime CreatedAt { get; set; }
	public int CreatedByUserId { get; set; }
	public UserDTO CreatedByUser { get; set; }
}

public class UserDTO
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Surname { get; set; }
	public string Email { get; set; }
	public string Role { get; set; }
	public DateTime CreatedAt { get; set; }

}

public class CustomerSearchDTO
{
	public int Id { get; set; }
	public string Name_Surname { get; set; }
	public string Name { get; set; }
	public string Surname { get; set; }
	public string PhoneNumber { get; set; }
	public string Email { get; set; }
	public string Company { get; set; }
	public string Department { get; set; }
	public string Position { get; set; }
	public DateTime CreatedAt { get; set; }
	public int CreatedByUserId { get; set; }
	public User? CreatedByUser { get; set; }

}