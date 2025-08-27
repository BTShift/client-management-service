using Microsoft.Extensions.DependencyInjection;

namespace ClientManagement.Application.Services;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add application services
        services.AddScoped<IClientApplicationService, ClientApplicationService>();
        
        return services;
    }
}