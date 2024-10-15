using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BDA.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace BDA.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PostController : ControllerBase
	{
		private readonly CustomerService _customerService;

		private readonly AppDbContext _context;
		private readonly ExcelService _excelService;
		private readonly ILogger<PostController> _logger;
		private readonly CsvService _csvService;
		private static List<Customers> _tempCustomers = new List<Customers>(); // Временное хранилище
		private static int _lastId = 0;
		private readonly IMapper _mapper;
		public PostController(AppDbContext context, ExcelService excelService, ILogger<PostController> logger, CsvService csvService, IMapper mapper)
		{
			_context = context;
			_excelService = excelService;
			_logger = logger;
			_csvService = csvService;
			_mapper = mapper;
			
		}

		[HttpGet("GETDTO")]
		public ActionResult<IEnumerable<CustomerDTO>> GetCustomersdto()
		{
			var customers = _context.Customers.Include(c => c.CreatedByUser).ToList();
			var customerDTOs = _mapper.Map<IEnumerable<CustomerDTO>>(customers);
			return Ok(customerDTOs);
		}

		[HttpGet("GETDTO_FOR_SEARCH")]
		public List<CustomerSearchDTO> GetCustomersS()
		{
			var customers = _context.Customers.ToList();

			var customerDtos = customers.Select(c => new CustomerSearchDTO
			{
				Id = c.Id,
				Name_Surname = c.Name + " " + c.Surname, 
				PhoneNumber = c.PhoneNumber,
				Email = c.Email
			}).ToList();

			return customerDtos;
		}


        [HttpGet("GETDATA")]
        public async Task<IActionResult> GetCustomers()
        {
            var stopwatch = Stopwatch.StartNew();
            var customers = await _context.Customers
                .Include(c => c.CreatedByUser)
                .ToListAsync();
            stopwatch.Stop();

           
            Console.WriteLine($"Time taken to get customers: {stopwatch.ElapsedMilliseconds} ms");

            return Ok(customers);
        }


        [HttpPost("CustomerD")]
        public async Task<IActionResult> CreateCustomerD([FromBody] Customers customer)
        {
          
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

         
            customer.CreatedAt = DateTime.UtcNow;


            customer.Leads ??= new List<Lead>();
            customer.ReceivedEmails ??= new List<Email>();


            if (customer.CreatedByUserId != null)
            {
                var user = await _context.Users.FindAsync(customer.CreatedByUserId);
                if (user == null)
                {
                    return NotFound($"User with ID {customer.CreatedByUserId} not found.");
                }
                customer.CreatedByUser = user;
            }


            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, customer);
        }

        [HttpGet("CustomerD")]
        public async Task<IActionResult> GetAllCustomers()
        {

            var customers = await _context.Customers.ToListAsync();

            if (customers == null || customers.Count == 0)
            {
                return NotFound("No customers found.");
            }
            return Ok(customers);
        }

        [HttpDelete("CustomerD/{id}")]
        public async Task<IActionResult> DeleteCustomerD(int id)
        {

            if (id == 0)
            {
                return BadRequest("Invalid customer ID.");
            }


            var customerInDb = await _context.Customers.FindAsync(id);


            if (customerInDb == null)
            {
                return NotFound($"Customer with Id {id} not found.");
            }


            _context.Customers.Remove(customerInDb);
            await _context.SaveChangesAsync();


            return Ok($"Customer with Id {id} deleted successfully.");
        }


        [HttpPut("CustomerD/{id}")]
        public async Task<IActionResult> UpdateCustomerD(int id, [FromBody] Customers updatedCustomer)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

           
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

          
            existingCustomer.Name = updatedCustomer.Name;
            existingCustomer.Surname = updatedCustomer.Surname;
            existingCustomer.PhoneNumber = updatedCustomer.PhoneNumber;
            existingCustomer.Email = updatedCustomer.Email;
            existingCustomer.Company = updatedCustomer.Company;
            existingCustomer.Department = updatedCustomer.Department;
            existingCustomer.Position = updatedCustomer.Position;

            
            if (updatedCustomer.CreatedByUserId != null)
            {
                var user = await _context.Users.FindAsync(updatedCustomer.CreatedByUserId);
                if (user == null)
                {
                    return NotFound($"User with ID {updatedCustomer.CreatedByUserId} not found.");
                }
                existingCustomer.CreatedByUser = user; 
            }

           
            if (updatedCustomer.Leads != null)
            {
                existingCustomer.Leads = updatedCustomer.Leads;
            }

            if (updatedCustomer.ReceivedEmails != null)
            {
                existingCustomer.ReceivedEmails = updatedCustomer.ReceivedEmails;
            }

         
            _context.Customers.Update(existingCustomer);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }




        [HttpGet("{id}")]
		public async Task<IActionResult> GetCustomerById(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null)
			{
				return NotFound();
			}
			return Ok(customer);
		}


        


        // POST: api/Post/ImportExcel
        [HttpPost("ImportExcel")]
		public async Task<IActionResult> ImportExcel(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded.");

			// Фильтрация по допустимым типам файлов (XLSX и CSV)
			var allowedExtensions = new[] { ".xlsx", ".csv" };
			var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (!allowedExtensions.Contains(extension))
			{
				return BadRequest("Invalid file format. Only .xlsx and .csv files are allowed.");
			}

			
			var filePath = Path.GetTempFileName();
			try
			{
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				
				var customers = _excelService.ImportAllExcelSheets(filePath); // Предполагается, что ваш сервис обрабатывает оба типа файлов

				
				_context.Customers.AddRange(customers);
				await _context.SaveChangesAsync();

			
				//string htmlContent = @"
    //    <span class='badge bg-primary' style='color: red;font-size: 50px;text-align: center;'>Data imported successfully!</span>
    //    <script>
    //        setTimeout(function() {
    //            window.location.href = 'http://localhost/demo/'; // Redirect to Google
    //        }, 2000); // Redirect after 2 seconds
    //    </script>";

				return Content("Succesfully");
			}
			finally
			{
				
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}
		}



		[HttpGet("Search")]
		public async Task<IActionResult> Search(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return BadRequest("Search query cannot be empty.");
			}

			try
			{
				
				var customers = await _context.Customers
					.Where(c => c.Name.Contains(query) ||
								c.PhoneNumber.Contains(query) ||
								c.Surname.Contains(query))
					.ToListAsync();

				return Ok(customers);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error searching for customers.");
				return StatusCode(500, "An error occurred while searching for customers.");
			}
		}


		[HttpGet("searchDTO")]
		public ActionResult<List<CustomerSearchDTO>> SearchCustomers(string searchTerm)
		{
			
			var customers = _context.Customers
				.Where(c => (c.Name + " " + c.Surname).Contains(searchTerm) ||
							 c.PhoneNumber.Contains(searchTerm) ||
							 c.Email.Contains(searchTerm))
				.ToList();

			// Mapping to DTO
			var customerDtos = customers.Select(c => new CustomerSearchDTO
			{
				Id = c.Id,
				Name_Surname = c.Name + " " + c.Surname, // Combine Name and Surname
				Name = c.Name,
				Surname = c.Surname,
				PhoneNumber = c.PhoneNumber,
				Email = c.Email,
				Company = c.Company,
				Department = c.Department,
				Position = c.Position,
				CreatedAt = c.CreatedAt,
				CreatedByUserId = (int)c.CreatedByUserId,
				CreatedByUser = c.CreatedByUser // If you want to include user details
			}).ToList();

			return Ok(customerDtos);
		}



        [HttpPost("TEMP DATA")]
		public async Task<ActionResult<Customers>> SendCustomer(Customers customer)
		{
			customer.Id = ++_lastId;
			// Добавляем клиента в временное хранилище
			_tempCustomers.Add(customer);

			
			return CreatedAtAction(nameof(GetCustomers), new { id = customer.Id }, customer);
		}

		[HttpGet("temp")]
		public ActionResult<IEnumerable<Customers>> GetTempCustomers()
		{
			// Возвращаем временные объекты
			return Ok(_tempCustomers);
		}

		[HttpDelete("temp/{id}")]
		public ActionResult DeleteTempCustomer(int id)
		{
			// Находим клиента с указанным ID
			var customer = _tempCustomers.FirstOrDefault(c => c.Id == id);
			if (customer == null)
			{
				return NotFound();
			}

			// Удаляем клиента из временного хранилища
			_tempCustomers.Remove(customer);
			return NoContent();
		}


		[HttpPost("save")]
		public async Task<ActionResult> SaveCustomers()
		{
			try
			{
				
				foreach (var tempCustomer in _tempCustomers)
				{
					tempCustomer.Id = 0; 
				}

				
				_context.Customers.AddRange(_tempCustomers);
				await _context.SaveChangesAsync();

				
				_tempCustomers.Clear();

				return Ok();
			}
			catch (Exception ex)
			{
				
				var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
				return StatusCode(500, $"Ошибка при сохранении данных: {innerException}");
			}
		}
		/////////////////////////////////////////////


		[HttpGet("withUserNames")]
		public async Task<IActionResult> GetEmailsWithUserNames()
		{
			var result = await _context.Emails
				.Include(e => e.SentByUser) // Связываем Email с User
				.Select(e => new
				{
					UserName = e.SentByUser.Name, // Имя пользователя
					EmailSubject = e.Subject // Тема письма
				})
				.ToListAsync();

			return Ok(result); // Возвращаем результат в формате JSON
		}

		[HttpGet("GetUserByCustomerId/{customerId}")]
		public async Task<IActionResult> GetUserByCustomerId(int customerId)
		{
			// Получаем клиента с пользователем по Id клиента
			var customer = await _context.Customers
				.Include(c => c.CreatedByUser) 
				.FirstOrDefaultAsync(c => c.Id == customerId);

			if (customer == null || customer.CreatedByUser == null)
			{
				return NotFound("Customer or user not found");
			}

			
			return Ok(new
			{
				UserName = customer.CreatedByUser.Name,
				surname = customer.CreatedByUser.Email
			});
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("admin-only")]
		public IActionResult AdminOnly()
		{
			return Ok("This is an admin-only endpoint.");
		}

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customers updatedCustomer)
        {
            if (id != updatedCustomer.Id) 
            {
                return BadRequest("Customer ID mismatch");
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

           
            customer.Name = updatedCustomer.Name;
            customer.Surname = updatedCustomer.Surname;
            customer.PhoneNumber = updatedCustomer.PhoneNumber;
            customer.Email = updatedCustomer.Email;
            customer.Company = updatedCustomer.Company;
            customer.Department = updatedCustomer.Department;
            customer.Position = updatedCustomer.Position;

          
            // customer.CreatedAt = DateTime.UtcNow; 

          
            await _context.SaveChangesAsync();

            return NoContent(); 
        }


    }
}
