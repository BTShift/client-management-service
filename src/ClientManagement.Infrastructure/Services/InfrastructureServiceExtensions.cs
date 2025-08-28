using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using ClientManagement.Application.Interfaces;
using ClientManagement.Infrastructure.Repositories;
using ClientManagement.Infrastructure.Data;

namespace ClientManagement.Infrastructure.Services;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Database Context - using PG* environment variables
        var connectionString = BuildConnectionString();
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ClientManagementDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Messaging infrastructure can be added here when needed
        // Example: services.AddShiftMessaging(configuration);

        // Add repository services
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IClientGroupRepository, ClientGroupRepository>();
        
        // Add user context service
        services.AddScoped<IUserContext<Grpc.Core.ServerCallContext>, GrpcUserContext>();

        return services;
    }

    private static string BuildConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("PGHOST");
        var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("PGDATABASE");
        var username = Environment.GetEnvironmentVariable("PGUSER");
        var password = Environment.GetEnvironmentVariable("PGPASSWORD");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(database) || 
            string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            // Return empty string if database is not configured (optional for some services)
            return string.Empty;
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}