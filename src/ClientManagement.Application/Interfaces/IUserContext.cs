namespace ClientManagement.Application.Interfaces;

/// <summary>
/// Provides access to the current user context
/// </summary>
public interface IUserContext<TContext> where TContext : class
{
    /// <summary>
    /// Gets the current user's ID or email from the context
    /// </summary>
    string? GetUserIdentity(TContext context);
    
    /// <summary>
    /// Gets the current tenant ID from the context
    /// </summary>
    string? GetTenantId(TContext context);
}