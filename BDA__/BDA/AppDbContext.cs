using Microsoft.EntityFrameworkCore;

using BDA.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BDA
{
	public class AppDbContext : IdentityDbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}
		public DbSet<User> Users { get; set; }
		public DbSet<Customers> Customers { get; set; }
		public DbSet<Email> Emails { get; set; }
		public DbSet<Lead> Leads { get; set; }
		public DbSet<LeadStageHistory> LeadStageHistories { get; set; }
		public DbSet<TempCustomer> TempCustomers { get; set; }
        public DbSet<VerificationToken> VerificationTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// User 
			modelBuilder.Entity<Customers>()
				.HasOne(c => c.CreatedByUser)
				.WithMany(u => u.Customers)
				.HasForeignKey(c => c.CreatedByUserId)
				.OnDelete(DeleteBehavior.Restrict); 

			// Customer 
			modelBuilder.Entity<Lead>()
				.HasOne(l => l.Customers)
				.WithMany(c => c.Leads)
				.HasForeignKey(l => l.CustomerId)
				.OnDelete(DeleteBehavior.Cascade); 

			// User to Emails
			modelBuilder.Entity<Email>()
				.HasOne(e => e.SentByUser)
				.WithMany(u => u.SentEmails)
				.HasForeignKey(e => e.SentByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			
			modelBuilder.Entity<Email>()
				.HasOne(e => e.SentToCustomer)
				.WithMany(c => c.ReceivedEmails)
				.HasForeignKey(e => e.SentToCustomerId)
				.OnDelete(DeleteBehavior.Cascade);

			// LeadStageHistory 
			modelBuilder.Entity<LeadStageHistory>()
				.HasOne(ls => ls.ChangedByUser)
				.WithMany(u => u.LeadStageHistories)
				.HasForeignKey(ls => ls.ChangedByUserId)
				.OnDelete(DeleteBehavior.Restrict);

			// Lead 
			modelBuilder.Entity<LeadStageHistory>()
				.HasOne(ls => ls.Lead)
				.WithMany(l => l.LeadStageHistories)
				.HasForeignKey(ls => ls.LeadId)
				.OnDelete(DeleteBehavior.Cascade);

			// Lead
			modelBuilder.Entity<Lead>()
				.HasOne(l => l.Agent)
				.WithMany(u => u.Leads)
				.HasForeignKey(l => l.AgentId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
