using Microsoft.EntityFrameworkCore;
using ClientManagement.Domain.Entities;
using ClientManagement.Infrastructure.Data;
using ClientManagement.Application.Interfaces;

namespace ClientManagement.Infrastructure.Repositories;

public class ClientGroupRepository : IClientGroupRepository
{
    private readonly ClientManagementDbContext _context;

    public ClientGroupRepository(ClientManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ClientGroup> CreateGroupAsync(ClientGroup group)
    {
        group.Id = Guid.NewGuid();
        group.CreatedAt = DateTime.UtcNow;
        group.UpdatedAt = DateTime.UtcNow;
        
        _context.ClientGroups.Add(group);
        await _context.SaveChangesAsync();
        
        return group;
    }

    public async Task<ClientGroup?> GetGroupByIdAsync(Guid groupId, string tenantId)
    {
        return await _context.ClientGroups
            .Include(g => g.ClientGroupMemberships)
            .ThenInclude(m => m.Client)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.TenantId == tenantId);
    }

    public async Task<ClientGroup?> UpdateGroupAsync(ClientGroup group)
    {
        var existingGroup = await _context.ClientGroups
            .FirstOrDefaultAsync(g => g.Id == group.Id && g.TenantId == group.TenantId);
        
        if (existingGroup == null)
            return null;
        
        existingGroup.Name = group.Name;
        existingGroup.Description = group.Description;
        existingGroup.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return existingGroup;
    }

    public async Task<bool> DeleteGroupAsync(Guid groupId, string tenantId, string? deletedBy = null)
    {
        var group = await _context.ClientGroups
            .FirstOrDefaultAsync(g => g.Id == groupId && g.TenantId == tenantId);
        
        if (group == null)
            return false;
        
        group.IsDeleted = true;
        group.DeletedAt = DateTime.UtcNow;
        group.DeletedBy = deletedBy;
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<(IList<ClientGroup> Items, int TotalCount)> ListGroupsAsync(
        string tenantId, 
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null)
    {
        var query = _context.ClientGroups
            .Where(g => g.TenantId == tenantId);
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(g => 
                g.Name.Contains(searchTerm) || 
                (g.Description != null && g.Description.Contains(searchTerm)));
        }
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(g => g.ClientGroupMemberships)
            .ToListAsync();
        
        return (items, totalCount);
    }

    public async Task<bool> GroupNameExistsAsync(string name, string tenantId, Guid? excludeGroupId = null)
    {
        var query = _context.ClientGroups
            .Where(g => g.TenantId == tenantId && g.Name == name);
        
        if (excludeGroupId.HasValue)
        {
            query = query.Where(g => g.Id != excludeGroupId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<bool> AddClientToGroupAsync(Guid clientId, Guid groupId, string tenantId, string? addedBy = null)
    {
        // Verify both client and group exist and belong to the same tenant
        var clientExists = await _context.Clients
            .AnyAsync(c => c.Id == clientId && c.TenantId == tenantId);
        
        var groupExists = await _context.ClientGroups
            .AnyAsync(g => g.Id == groupId && g.TenantId == tenantId);
        
        if (!clientExists || !groupExists)
            return false;
        
        // Check if membership already exists
        var membershipExists = await _context.ClientGroupMemberships
            .AnyAsync(m => m.ClientId == clientId && m.GroupId == groupId);
        
        if (membershipExists)
            return true; // Already a member
        
        var membership = new ClientGroupMembership
        {
            ClientId = clientId,
            GroupId = groupId,
            JoinedAt = DateTime.UtcNow,
            AddedBy = addedBy
        };
        
        _context.ClientGroupMemberships.Add(membership);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> RemoveClientFromGroupAsync(Guid clientId, Guid groupId, string tenantId)
    {
        var membership = await _context.ClientGroupMemberships
            .Include(m => m.Client)
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m => 
                m.ClientId == clientId && 
                m.GroupId == groupId &&
                m.Client.TenantId == tenantId &&
                m.Group.TenantId == tenantId);
        
        if (membership == null)
            return false;
        
        _context.ClientGroupMemberships.Remove(membership);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<IList<Client>> GetGroupClientsAsync(Guid groupId, string tenantId)
    {
        return await _context.ClientGroupMemberships
            .Include(m => m.Client)
            .Where(m => m.GroupId == groupId && m.Group.TenantId == tenantId)
            .Select(m => m.Client)
            .ToListAsync();
    }

    public async Task<IList<ClientGroup>> GetClientGroupsAsync(Guid clientId, string tenantId)
    {
        return await _context.ClientGroupMemberships
            .Include(m => m.Group)
            .Where(m => m.ClientId == clientId && m.Client.TenantId == tenantId)
            .Select(m => m.Group)
            .ToListAsync();
    }

    public async Task<bool> IsClientInGroupAsync(Guid clientId, Guid groupId, string tenantId)
    {
        return await _context.ClientGroupMemberships
            .Include(m => m.Client)
            .Include(m => m.Group)
            .AnyAsync(m => 
                m.ClientId == clientId && 
                m.GroupId == groupId &&
                m.Client.TenantId == tenantId &&
                m.Group.TenantId == tenantId);
    }
}