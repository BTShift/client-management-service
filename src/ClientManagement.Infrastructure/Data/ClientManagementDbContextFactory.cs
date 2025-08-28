using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClientManagement.Infrastructure.Data;

public class ClientManagementDbContextFactory : IDesignTimeDbContextFactory<ClientManagementDbContext>
{
    public ClientManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClientManagementDbContext>();
        
        // For design-time migrations, use a default connection string
        // This will be overridden by actual environment variables at runtime
        var connectionString = "Host=localhost;Port=5432;Database=clientmanagement_dev;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new ClientManagementDbContext(optionsBuilder.Options);
    }
}