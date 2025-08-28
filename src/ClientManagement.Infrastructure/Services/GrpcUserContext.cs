using Grpc.Core;
using Microsoft.AspNetCore.Http;
using ClientManagement.Application.Interfaces;

namespace ClientManagement.Infrastructure.Services;

/// <summary>
/// Implementation of IUserContext for gRPC services that extracts user information from the call context
/// </summary>
public class GrpcUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public GrpcUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return "System";
            
        // Try to get from headers
        if (httpContext.Request.Headers.TryGetValue("x-user-id", out var userId) && !string.IsNullOrEmpty(userId))
        {
            return userId.ToString();
        }
        
        // Try to get from claims
        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst("sub") ?? user.FindFirst("user_id");
            if (userIdClaim != null)
            {
                return userIdClaim.Value;
            }
        }
        
        // For development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return "dev-user";
        }
        
        return "System";
    }
    
    public string GetCurrentUserName()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return "System";
            
        // Try to get from headers
        if (httpContext.Request.Headers.TryGetValue("x-user-email", out var userEmail) && !string.IsNullOrEmpty(userEmail))
        {
            return userEmail.ToString();
        }
        
        // Try to get from claims
        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Try email claim
            var emailClaim = user.FindFirst("email") ?? user.FindFirst("preferred_username");
            if (emailClaim != null)
            {
                return emailClaim.Value;
            }
            
            // Fall back to name
            if (!string.IsNullOrEmpty(user.Identity.Name))
            {
                return user.Identity.Name;
            }
        }
        
        // For development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return "dev-user";
        }
        
        return "System";
    }
    
    public string GetCurrentTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return "default-tenant";
            
        // Try to get from headers
        if (httpContext.Request.Headers.TryGetValue("x-tenant-id", out var tenantId) && !string.IsNullOrEmpty(tenantId))
        {
            return tenantId.ToString();
        }
        
        // Try to get from claims
        var user = httpContext.User;
        if (user != null)
        {
            var tenantClaim = user.FindFirst("tenant_id") ?? user.FindFirst("tenantId");
            if (tenantClaim != null)
            {
                return tenantClaim.Value;
            }
        }
        
        // For development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return "default-tenant";
        }
        
        return "default-tenant";
    }
    
    public bool IsAuthenticated()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}