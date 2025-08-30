using MassTransit;
using Microsoft.EntityFrameworkCore;
using ClientManagement.Infrastructure.Data;
using ClientManagement.Domain.Entities;
using ClientManagement.Contract.Commands;
using ClientManagement.Contract.Events;

namespace ClientManagement.Api.Consumers;

public class InitializeClientManagementConsumer : IConsumer<InitializeClientManagementCommand>
{
    private readonly ILogger<InitializeClientManagementConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public InitializeClientManagementConsumer(
        ILogger<InitializeClientManagementConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task Consume(ConsumeContext<InitializeClientManagementCommand> context)
    {
        var command = context.Message;
        
        _logger.LogInformation("Initializing client management for tenant {TenantId} with correlation {CorrelationId}", 
            command.TenantId, command.CorrelationId);

        try
        {
            // Create tenant-specific database context
            var tenantConnectionString = BuildTenantConnectionString(command.DatabaseName);
            
            using var scope = _serviceProvider.CreateScope();
            
            // Create DbContext with tenant-specific connection string
            var options = new DbContextOptionsBuilder<ClientManagementDbContext>()
                .UseNpgsql(tenantConnectionString)
                .Options;
            
            await using var dbContext = new ClientManagementDbContext(options);
            
            // Check if this is a fresh database or one that needs migration
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Applying {Count} migrations for tenant {TenantId}", 
                    pendingMigrations.Count(), command.TenantId);
                
                // Apply migrations to ensure database schema is properly created
                // This creates the schema with correct column names from migrations
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                // Database already has schema (either from migrations or EnsureCreated)
                // Check if we have the migration history table
                var sql = @"
                    SELECT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_schema = 'client_management' 
                        AND table_name = '__EFMigrationsHistory'
                    )";
                
                var connection = dbContext.Database.GetDbConnection();
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                var result = await command.ExecuteScalarAsync();
                var hasMigrationHistory = result != null && (bool)result;
                await connection.CloseAsync();
                
                if (!hasMigrationHistory)
                {
                    _logger.LogWarning("Database exists without migration history for tenant {TenantId}. " +
                        "This database was likely created with EnsureCreated. " +
                        "Future schema updates may require manual intervention.", command.TenantId);
                }
                else
                {
                    _logger.LogInformation("Database schema already up-to-date for tenant {TenantId}", 
                        command.TenantId);
                }
            }
            
            // Initialize default client management data
            await InitializeDefaultData(dbContext, command.TenantId);
            
            _logger.LogInformation("Successfully initialized client management for tenant {TenantId}", command.TenantId);
            
            // Publish success event using the MassTransit context
            await context.Publish(new ClientManagementInitializedEvent
            {
                CorrelationId = command.CorrelationId,
                Timestamp = DateTime.UtcNow,
                TenantId = command.TenantId,
                SchemaCreated = true,
                InitializedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize client management for tenant {TenantId}", command.TenantId);
            throw; // Let the base class handle retry logic
        }
    }

    private static string BuildTenantConnectionString(string databaseName)
    {
        var host = Environment.GetEnvironmentVariable("PGHOST");
        var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var username = Environment.GetEnvironmentVariable("PGUSER");
        var password = Environment.GetEnvironmentVariable("PGPASSWORD");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("Database connection parameters are not configured");
        }

        return $"Host={host};Port={port};Database={databaseName};Username={username};Password={password}";
    }

    private static async Task InitializeDefaultData(ClientManagementDbContext dbContext, string tenantId)
    {
        // Check if data already exists
        var existingGroups = await dbContext.ClientGroups.AnyAsync();
        if (existingGroups)
        {
            // Already initialized
            return;
        }

        // Create default client groups
        var defaultGroups = new[]
        {
            new ClientGroup
            {
                TenantId = tenantId,
                Name = "Premium",
                Description = "Premium clients with comprehensive service packages",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ClientGroup
            {
                TenantId = tenantId,
                Name = "Standard",
                Description = "Standard clients with regular service packages",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ClientGroup
            {
                TenantId = tenantId,
                Name = "Basic",
                Description = "Basic clients with essential service packages",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new ClientGroup
            {
                TenantId = tenantId,
                Name = "VIP",
                Description = "VIP clients with priority support and premium services",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await dbContext.ClientGroups.AddRangeAsync(defaultGroups);
        await dbContext.SaveChangesAsync();
    }
}