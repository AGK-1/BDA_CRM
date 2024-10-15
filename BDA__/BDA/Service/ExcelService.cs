using BDA;
using BDA.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

public class ExcelService
{
	public List<Customers> ImportAllExcelSheets(string filePath)
	{
		var allCustomers = new List<Customers>();

		using (var package = new ExcelPackage(new FileInfo(filePath)))
		{
			foreach (var worksheet in package.Workbook.Worksheets)
			{
				// Assuming first row contains headers, and the data starts from row 2
				for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
				{
					var customer = new Customers
					{
						Name = worksheet.Cells[row, 1].Value?.ToString(),
						Surname = worksheet.Cells[row, 2].Value?.ToString(),
						PhoneNumber = worksheet.Cells[row, 3].Value?.ToString(),
						Email = worksheet.Cells[row, 4].Value?.ToString(),
						Company = worksheet.Cells[row, 5].Value?.ToString(),
						Department = worksheet.Cells[row, 6].Value?.ToString(),
						Position = worksheet.Cells[row, 7].Value?.ToString()
					};

					allCustomers.Add(customer);
				}
			}
		}

		return allCustomers;
	}

	public List<Customers> ImportAllExcelSh(string filePath)
	{
		var allCustomers = new List<Customers>();

		using (var package = new ExcelPackage(new FileInfo(filePath)))
		{
			foreach (var worksheet in package.Workbook.Worksheets)
			{
				// Loop through all rows starting from row 2 (assuming row 1 is the header)
				for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
				{
					// Check if the key columns (e.g., Name and Surname) are empty, skip the row if they are
					var name = worksheet.Cells[row, 1].Value?.ToString();
					var surname = worksheet.Cells[row, 2].Value?.ToString();
					var phoneNumber = worksheet.Cells[row, 3].Value?.ToString();
					var email = worksheet.Cells[row, 4].Value?.ToString();
					var company = worksheet.Cells[row, 5].Value?.ToString();
					var department = worksheet.Cells[row, 6].Value?.ToString();
					var position = worksheet.Cells[row, 7].Value?.ToString();
				
					if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(surname) && string.IsNullOrWhiteSpace(phoneNumber) 
						&& string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(department)
						&& string.IsNullOrWhiteSpace(position))
					{
						continue; // Skip the row if both Name and Surname are empty
					}

					// If the row is not empty, create a new Customer object
					var customer = new Customers
					{
						Name = name,
						Surname = surname,
						PhoneNumber = phoneNumber,
						Email = email,
						Company = company,
						Department = department,
						Position = position
					};

					allCustomers.Add(customer); // Add the valid customer to the list
				}
			}
		}

		return allCustomers;
	}
}