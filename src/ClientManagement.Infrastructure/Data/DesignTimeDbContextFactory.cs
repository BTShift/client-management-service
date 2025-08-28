using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ClientManagement.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ClientManagementDbContext>
{
    public ClientManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClientManagementDbContext>();
        
        // Use a connection string for design-time operations
        // This will be replaced with actual connection strings from configuration in production
        var connectionString = Environment.GetEnvironmentVariable("PGCONNECTION") ?? 
            "Host=localhost;Database=client_management_dev;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new ClientManagementDbContext(optionsBuilder.Options);
    }
}