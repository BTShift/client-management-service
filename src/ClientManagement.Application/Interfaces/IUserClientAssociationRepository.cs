using ClientManagement.Domain.Entities;

namespace ClientManagement.Application.Interfaces;

public interface IUserClientAssociationRepository
{
    Task<UserClientAssociation?> GetAssociationAsync(string userId, Guid clientId, string tenantId);
    Task<UserClientAssociation> AddAssociationAsync(UserClientAssociation association);
    Task<bool> RemoveAssociationAsync(string userId, Guid clientId, string tenantId);
    Task<List<UserClientAssociation>> GetClientUsersAsync(Guid clientId, string tenantId, int page, int pageSize);
    Task<int> GetClientUserCountAsync(Guid clientId, string tenantId);
    Task<List<UserClientAssociation>> GetUserClientsAsync(string userId, string tenantId, int page, int pageSize);
    Task<int> GetUserClientCountAsync(string userId, string tenantId);
    Task<bool> UserExistsAsync(string userId, string tenantId);
}