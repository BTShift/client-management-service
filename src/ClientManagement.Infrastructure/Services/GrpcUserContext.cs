using Grpc.Core;
using ClientManagement.Application.Interfaces;

namespace ClientManagement.Infrastructure.Services;

/// <summary>
/// Implementation of IUserContext for gRPC services
/// </summary>
public class GrpcUserContext : IUserContext<ServerCallContext>
{
    public string? GetUserIdentity(ServerCallContext context)
    {
        // Try to get user ID from headers
        var userId = context.RequestHeaders.GetValue("x-user-id");
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }
        
        // Try to get user email from headers
        var userEmail = context.RequestHeaders.GetValue("x-user-email");
        if (!string.IsNullOrEmpty(userEmail))
        {
            return userEmail;
        }
        
        // Try to get from authorization context if using JWT
        var httpContext = context.GetHttpContext();
        var user = httpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Try to get user ID from claims
            var userIdClaim = user.FindFirst("sub") ?? user.FindFirst("user_id");
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }
            
            // Fall back to email or name
            var emailClaim = user.FindFirst("email") ?? user.FindFirst("preferred_username");
            if (emailClaim != null)
            {
                return emailClaim.Value;
            }
            
            return user.Identity.Name;
        }
        
        // For development/testing, return a default user
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return "dev-user";
        }
        
        return null; // Unknown user
    }
    
    public string? GetTenantId(ServerCallContext context)
    {
        // Try to get from metadata/headers
        var tenantIdEntry = context.RequestHeaders.GetValue("x-tenant-id");
        if (!string.IsNullOrEmpty(tenantIdEntry))
        {
            return tenantIdEntry;
        }
        
        // Try to get from HTTP context if available
        var httpContext = context.GetHttpContext();
        if (httpContext?.User != null)
        {
            var tenantClaim = httpContext.User.FindFirst("tenant_id") 
                           ?? httpContext.User.FindFirst("tenantId");
            if (tenantClaim != null)
            {
                return tenantClaim.Value;
            }
        }
        
        // For development/testing, use a default tenant
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return "default-tenant";
        }
        
        return null;
    }
}