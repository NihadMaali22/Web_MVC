using Microsoft.EntityFrameworkCore;
using lily.Models;

namespace lily.Data;

public class ToyStoreContext : DbContext
{
    public ToyStoreContext()
    {
    }

    public ToyStoreContext(DbContextOptions<ToyStoreContext> options)
        : base(options)
    {
    }
   

    public DbSet<Order> Orders { get; set; }
    public DbSet<Toy> Toys { get; set; }
    public DbSet<User> Users { get; set; }

    
    
}
