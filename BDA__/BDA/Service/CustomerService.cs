using BDA;

public class CustomerService
{
	private readonly AppDbContext _context;

	public CustomerService(AppDbContext context)
	{
		_context = context;
	}


}
