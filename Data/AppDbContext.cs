using Microsoft.EntityFrameworkCore;
using FabrikaBackend.Models;

namespace FabrikaBackend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Orders> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Personnel> Personnels { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Machine> Machines { get; set; }
    public DbSet<Stock> Stocks { get; set; } 
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
    }
}