using Microsoft.EntityFrameworkCore;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;
using ClientManagement.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace ClientManagement.Infrastructure.Repositories;

public class UserClientAssociationRepository : IUserClientAssociationRepository
{
    private readonly ClientManagementDbContext _context;
    private readonly ILogger<UserClientAssociationRepository> _logger;

    public UserClientAssociationRepository(ClientManagementDbContext context, ILogger<UserClientAssociationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserClientAssociation?> GetAssociationAsync(string userId, Guid clientId, string tenantId)
    {
        return await _context.UserClientAssociations
            .Include(a => a.Client)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.ClientId == clientId && a.TenantId == tenantId);
    }

    public async Task<UserClientAssociation> AddAssociationAsync(UserClientAssociation association)
    {
        _context.UserClientAssociations.Add(association);
        await _context.SaveChangesAsync();
        return association;
    }

    public async Task<bool> RemoveAssociationAsync(string userId, Guid clientId, string tenantId)
    {
        var association = await GetAssociationAsync(userId, clientId, tenantId);
        if (association == null)
        {
            return false;
        }

        _context.UserClientAssociations.Remove(association);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserClientAssociation>> GetClientUsersAsync(Guid clientId, string tenantId, int page, int pageSize)
    {
        return await _context.UserClientAssociations
            .Include(a => a.Client)
            .Where(a => a.ClientId == clientId && a.TenantId == tenantId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetClientUserCountAsync(Guid clientId, string tenantId)
    {
        return await _context.UserClientAssociations
            .CountAsync(a => a.ClientId == clientId && a.TenantId == tenantId);
    }

    public async Task<List<UserClientAssociation>> GetUserClientsAsync(string userId, string tenantId, int page, int pageSize)
    {
        return await _context.UserClientAssociations
            .Include(a => a.Client)
            .Where(a => a.UserId == userId && a.TenantId == tenantId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUserClientCountAsync(string userId, string tenantId)
    {
        return await _context.UserClientAssociations
            .CountAsync(a => a.UserId == userId && a.TenantId == tenantId);
    }

    public async Task<bool> UserExistsAsync(string userId, string tenantId)
    {
        // TODO: This should validate against Identity service
        // For now, return true as a placeholder
        _logger.LogWarning("UserExistsAsync called - Identity service integration pending");
        return await Task.FromResult(true);
    }
}