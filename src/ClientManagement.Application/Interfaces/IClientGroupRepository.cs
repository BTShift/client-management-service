using ClientManagement.Domain.Entities;

namespace ClientManagement.Application.Interfaces;

public interface IClientGroupRepository
{
    Task<ClientGroup> CreateGroupAsync(ClientGroup group);
    Task<ClientGroup?> GetGroupByIdAsync(Guid groupId, string tenantId);
    Task<ClientGroup?> UpdateGroupAsync(ClientGroup group);
    Task<bool> DeleteGroupAsync(Guid groupId, string tenantId, string? deletedBy = null);
    Task<(IList<ClientGroup> Items, int TotalCount)> ListGroupsAsync(
        string tenantId, 
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null);
    Task<bool> GroupNameExistsAsync(string name, string tenantId, Guid? excludeGroupId = null);
    
    // Client-Group membership operations
    Task<bool> AddClientToGroupAsync(Guid clientId, Guid groupId, string tenantId, string? addedBy = null);
    Task<bool> RemoveClientFromGroupAsync(Guid clientId, Guid groupId, string tenantId);
    Task<IList<Client>> GetGroupClientsAsync(Guid groupId, string tenantId);
    Task<IList<ClientGroup>> GetClientGroupsAsync(Guid clientId, string tenantId);
    Task<bool> IsClientInGroupAsync(Guid clientId, Guid groupId, string tenantId);
}