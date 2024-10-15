using BDA;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using BDA.Models;
using System.Reflection;
using Microsoft.Office.Interop.Excel;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BDA.EmailSender;


var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Require email confirmation
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

var EmailCenderLine = new List<string> { "255.0.0.111", "255.168.100.120" };

//builder.Services.AddScoped<AuthService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


builder.Services.AddAuthorization();
//////////////////////////////////
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // Enable compression for HTTPS requests
});
// Add services ////////////////////
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Correct way to access Configuration

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<CsvService>();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
	options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
	});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Configure JWT Bearer Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:7123") // Replace with your frontend URL
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});








builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
var app = builder.Build();
app.UseMiddleware<IEmailSender_s>(EmailCenderLine);

app.UseSwagger();

app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
	c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});


app.UseResponseCompression();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseRouting();
app.UseHttpsRedirection();
// Make sure session is enabled.

app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.UseCors("AllowSpecificOrigin");
app.MapControllers();

app.Run();


public class SmtpSettings
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}