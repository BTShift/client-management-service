using ClientManagement.Domain.Entities;

namespace ClientManagement.Application.Services;

public interface IUserClientAssociationApplicationService
{
    Task<UserClientAssociation> AssignUserToClientAsync(string userId, Guid clientId, string tenantId, string assignedBy);
    Task<bool> RemoveUserFromClientAsync(string userId, Guid clientId, string tenantId);
    Task<(List<UserClientAssociation> associations, int totalCount)> GetClientUsersAsync(Guid clientId, string tenantId, int page, int pageSize);
    Task<(List<UserClientAssociation> associations, int totalCount)> GetUserClientsAsync(string userId, string tenantId, int page, int pageSize);
}