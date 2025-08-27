using Microsoft.EntityFrameworkCore;

namespace ClientManagement.Infrastructure;

public class ClientManagementDbContext : DbContext
{
    public ClientManagementDbContext(DbContextOptions<ClientManagementDbContext> options)
        : base(options)
    {
    }

    // Add DbSets here as entities are created
    // Example: public DbSet<Client> Clients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entity mappings here
    }
}