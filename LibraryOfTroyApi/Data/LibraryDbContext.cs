using LibraryOfTroyApi.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryOfTroyApi.Data;

public class LibraryDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }
    
    public DbSet<Book> Books { get; set; }
    public DbSet<CustomerReview> CustomerReviews { get; set; }
    public DbSet<CheckOutEvent> CheckOuts { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // This is critical for Identity tables
        
        modelBuilder.Entity<Customer>()
            .HasIndex(e => e.UserName)
            .IsUnique();
            
        // Configure the relationship between ApplicationUser and Customer
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(a => a.Customer)
            .WithOne()
            .HasForeignKey<ApplicationUser>(a => a.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}