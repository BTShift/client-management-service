using Microsoft.Extensions.Logging;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;

namespace ClientManagement.Application.Services;

public class ClientGroupApplicationService : IClientGroupApplicationService
{
    private readonly IClientGroupRepository _repository;
    private readonly ILogger<ClientGroupApplicationService> _logger;

    public ClientGroupApplicationService(IClientGroupRepository repository, ILogger<ClientGroupApplicationService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ClientGroup> CreateGroupAsync(string tenantId, string name, string? description)
    {
        _logger.LogInformation("Creating group {Name} for tenant {TenantId}", name, tenantId);
        
        // Check if group name already exists
        if (await _repository.GroupNameExistsAsync(name, tenantId))
        {
            throw new InvalidOperationException($"Group with name '{name}' already exists for this tenant");
        }
        
        var group = new ClientGroup
        {
            TenantId = tenantId,
            Name = name,
            Description = description
        };
        
        return await _repository.CreateGroupAsync(group);
    }

    public async Task<ClientGroup?> GetGroupByIdAsync(Guid groupId, string tenantId)
    {
        return await _repository.GetGroupByIdAsync(groupId, tenantId);
    }

    public async Task<ClientGroup?> UpdateGroupAsync(Guid groupId, string tenantId, string name, string? description)
    {
        _logger.LogInformation("Updating group {GroupId} for tenant {TenantId}", groupId, tenantId);
        
        var existingGroup = await _repository.GetGroupByIdAsync(groupId, tenantId);
        if (existingGroup == null)
        {
            return null;
        }
        
        // Check if new name already exists (excluding current group)
        if (existingGroup.Name != name && await _repository.GroupNameExistsAsync(name, tenantId, groupId))
        {
            throw new InvalidOperationException($"Group with name '{name}' already exists for this tenant");
        }
        
        existingGroup.Name = name;
        existingGroup.Description = description;
        
        return await _repository.UpdateGroupAsync(existingGroup);
    }

    public async Task<bool> DeleteGroupAsync(Guid groupId, string tenantId, string? deletedBy = null)
    {
        _logger.LogInformation("Deleting group {GroupId} for tenant {TenantId}", groupId, tenantId);
        return await _repository.DeleteGroupAsync(groupId, tenantId, deletedBy);
    }

    public async Task<(IList<ClientGroup> Groups, int TotalCount)> ListGroupsAsync(string tenantId, int page = 1, int pageSize = 20, string? searchTerm = null)
    {
        return await _repository.ListGroupsAsync(tenantId, page, pageSize, searchTerm);
    }

    public async Task<bool> AddClientToGroupAsync(Guid clientId, Guid groupId, string tenantId, string? addedBy = null)
    {
        _logger.LogInformation("Adding client {ClientId} to group {GroupId} for tenant {TenantId}", clientId, groupId, tenantId);
        return await _repository.AddClientToGroupAsync(clientId, groupId, tenantId, addedBy);
    }

    public async Task<bool> RemoveClientFromGroupAsync(Guid clientId, Guid groupId, string tenantId)
    {
        _logger.LogInformation("Removing client {ClientId} from group {GroupId} for tenant {TenantId}", clientId, groupId, tenantId);
        return await _repository.RemoveClientFromGroupAsync(clientId, groupId, tenantId);
    }

    public async Task<IList<Client>> GetGroupClientsAsync(Guid groupId, string tenantId)
    {
        return await _repository.GetGroupClientsAsync(groupId, tenantId);
    }

    public async Task<IList<ClientGroup>> GetClientGroupsAsync(Guid clientId, string tenantId)
    {
        return await _repository.GetClientGroupsAsync(clientId, tenantId);
    }
}