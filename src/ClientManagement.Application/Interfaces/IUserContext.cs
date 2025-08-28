namespace ClientManagement.Application.Interfaces;

/// <summary>
/// Provides access to the current user context for audit and tracking purposes
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the current user's ID from the authentication context
    /// </summary>
    string GetCurrentUserId();
    
    /// <summary>
    /// Gets the current user's name or email for display purposes
    /// </summary>
    string GetCurrentUserName();
    
    /// <summary>
    /// Gets the current tenant ID from the authentication context
    /// </summary>
    string GetCurrentTenantId();
    
    /// <summary>
    /// Checks if a user is currently authenticated
    /// </summary>
    bool IsAuthenticated();
}