using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Shift.Messaging.Infrastructure;
using MassTransit;

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

        // Add Shift Messaging Infrastructure (RabbitMQ with MassTransit)
        // Check if RabbitMQ is configured using the .NET configuration pattern
        var rabbitMqHost = configuration["RabbitMQ:Host"] ?? Environment.GetEnvironmentVariable("RabbitMQ__Host");
        if (!string.IsNullOrEmpty(rabbitMqHost))
        {
            services.AddShiftMessaging(configuration, consumers =>
            {
                // Register message consumers here
                // Example: consumers.AddConsumer<TenantCreatedEventConsumer>();
            });
        }

        // Add other infrastructure services
        // Example: services.AddScoped<IClientRepository, ClientRepository>();

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