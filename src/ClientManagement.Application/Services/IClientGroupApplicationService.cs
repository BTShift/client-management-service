using ClientManagement.Domain.Entities;

namespace ClientManagement.Application.Services;

public interface IClientGroupApplicationService
{
    Task<ClientGroup> CreateGroupAsync(string tenantId, string name, string? description);
    Task<ClientGroup?> GetGroupByIdAsync(Guid groupId, string tenantId);
    Task<ClientGroup?> UpdateGroupAsync(Guid groupId, string tenantId, string name, string? description);
    Task<bool> DeleteGroupAsync(Guid groupId, string tenantId, string? deletedBy = null);
    Task<(IList<ClientGroup> Groups, int TotalCount)> ListGroupsAsync(string tenantId, int page = 1, int pageSize = 20, string? searchTerm = null);
    
    // Client-Group membership operations
    Task<bool> AddClientToGroupAsync(Guid clientId, Guid groupId, string tenantId, string? addedBy = null);
    Task<bool> RemoveClientFromGroupAsync(Guid clientId, Guid groupId, string tenantId);
    Task<IList<Client>> GetGroupClientsAsync(Guid groupId, string tenantId);
    Task<IList<ClientGroup>> GetClientGroupsAsync(Guid clientId, string tenantId);
}