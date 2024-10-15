using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BDA.Models;
using BDA;

public class CsvService
{
	private readonly AppDbContext _context;

	public CsvService(AppDbContext context)
	{
		_context = context;
	}
	public IEnumerable<Customers> ImportCsvData(string filePath)
	{
		var customers = new List<Customers>();
		var lines = File.ReadAllLines(filePath);

		foreach (var line in lines.Skip(1)) // Пропускаем заголовок
		{
			var columns = line.Split(',');
			var customer = new Customers
			{
				Name = columns[0],
				Surname = columns[1],
				Email = columns[2],
				PhoneNumber = columns[3]
			};
			customers.Add(customer);
		}

		return customers;
	}
}
