using BDA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace BDA.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CustomerController : Controller
	{
		private readonly AppDbContext _context;
		private readonly ExcelService _excelService;
		private static List<Customers> _tempCustomers = new List<Customers>(); // Временное хранилище
		private static int _lastId = 0;
		public CustomerController(AppDbContext context, ExcelService excelService)
		{
			_context = context;
			_excelService = excelService;
		}

		[HttpPost("ImportExcel")]
		public async Task<IActionResult> ImportExcel(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest("No file uploaded.");

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

				// Import data from Excel or CSV
				var customers = _excelService.ImportAllExcelSh(filePath); // Assuming this returns a List<Customers>

				// Map Customers to TempCustomers
				var tempCustomers = customers.Select(c => new TempCustomer
				{
					Name = c.Name,
					Surname = c.Surname,
					PhoneNumber = c.PhoneNumber,
					Email = c.Email,
					Company = c.Company,
					Department = c.Department,
					Position = c.Position,
					CreatedAt = DateTime.UtcNow
				}).ToList();

				// Save to the temporary table
				_context.TempCustomers.AddRange(tempCustomers); // Use AddRange to add the list
				await _context.SaveChangesAsync(); // Save changes

				//string htmlContent = @"
    //             <span class='badge bg-primary' style='color: red;font-size: 50px;text-align: center;'>Data imported successfully!</span>
    //             <script>
    //             setTimeout(function() {
    //               window.location.href = 'http://localhost/demo/temp_x.html'; // Redirect to Google
    //              }, 1000); // Redirect after 2 seconds
    //              </script>";
				

				return Content("OK added Succesfully");
			}
			finally
			{
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}
			}
		}


		[HttpPost("TransferToCustomers")]
		public async Task<IActionResult> TransferToCustomers()
		{
			// Fetch all temporary customers
			var tempCustomers = await _context.TempCustomers.ToListAsync();

			// Check if there are temporary customers to transfer
			if (tempCustomers.Count == 0)
			{
				return BadRequest("No temporary customers found to transfer.");
			}

			// Get the current user's ID (adjust according to your auth system)
			int currentUserId = GetCurrentUserId();

			// Check if the user exists
			var userExists = await _context.Users.AnyAsync(u => u.Id == currentUserId);
			if (!userExists)
			{
				// Return a specific value indicating that the user does not exist
				return Ok(0); // Indicating that no transfer occurred due to the missing user
			}
			
			// Proceed with mapping and transferring customers
			var customers = tempCustomers.Select(tc => new Customers
			{
				Name = tc.Name,
				Surname = tc.Surname,
				PhoneNumber = tc.PhoneNumber,
				Email = tc.Email,
				Company = tc.Company,
				Department = tc.Department,
				Position = tc.Position,
				CreatedAt = DateTime.UtcNow,
				CreatedByUserId = currentUserId // Set the created by user ID
			}).ToList();

			
			// Add the mapped customers to the main Customers table
			_context.Customers.AddRange(customers);
			await _context.SaveChangesAsync();

			// Clean up: Remove all temporary customers
			_context.TempCustomers.RemoveRange(tempCustomers);
			await _context.SaveChangesAsync();

			return Ok("Temporary customers have been transferred to the main Customers table and cleaned up.");
		}


		private int GetCurrentUserId()
		{
			return 1; // Hardcoded ID for demonstration purposes
		}

        [Authorize]
        [HttpGet("TempCustomersGET")]
		public async Task<IActionResult> GetTempCustomers()
		{
			// Retrieve all temporary customers from the database
			var tempCustomers = await _context.TempCustomers.ToListAsync();

			if (tempCustomers == null || !tempCustomers.Any())
			{
				return NotFound("No temporary customers found.");
			}

			return Ok(tempCustomers); // Return the list of temporary customers
		}

		[HttpDelete("TempCustomersDEL/{id}")]
		public async Task<IActionResult> DeleteTempCustomer(int id)
		{
			// Find the temporary customer by ID
			var tempCustomer = await _context.TempCustomers.FindAsync(id);

			if (tempCustomer == null)
			{
				return NotFound("Temporary customer not found.");
			}

			// Remove the temporary customer
			_context.TempCustomers.Remove(tempCustomer);
			await _context.SaveChangesAsync();

			return Ok("Temporary customer deleted successfully.");
		}

		[HttpPut("UpdateTempCustomer/{id}")]
		public async Task<IActionResult> UpdateTempCustomer(int id, [FromBody] TempCustomer updatedTempCustomer)
		{
			if (updatedTempCustomer == null)
			{
				return BadRequest("Request body cannot be null.");
			}

			if (id != updatedTempCustomer.Id)
			{
				return BadRequest("ID in the URL does not match the ID in the body.");
			}

			var existingTempCustomer = await _context.TempCustomers.FindAsync(id);
			if (existingTempCustomer == null)
			{
				return NotFound("Customer not found.");
			}

			// Update fields
			existingTempCustomer.Name = updatedTempCustomer.Name;
			existingTempCustomer.Surname = updatedTempCustomer.Surname;
			existingTempCustomer.PhoneNumber = updatedTempCustomer.PhoneNumber;
			existingTempCustomer.Email = updatedTempCustomer.Email;
			existingTempCustomer.Company = updatedTempCustomer.Company;
			existingTempCustomer.Department = updatedTempCustomer.Department;
			existingTempCustomer.Position = updatedTempCustomer.Position;

			_context.TempCustomers.Update(existingTempCustomer);
			await _context.SaveChangesAsync();

			return Ok("Customer updated successfully.");
		}




	}
}