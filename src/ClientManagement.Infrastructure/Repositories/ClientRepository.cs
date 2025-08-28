using Microsoft.EntityFrameworkCore;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;
using ClientManagement.Infrastructure.Data;

namespace ClientManagement.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly ClientManagementDbContext _context;

    public ClientRepository(ClientManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Client> CreateAsync(Client client)
    {
        client.Id = Guid.NewGuid();
        client.CreatedAt = DateTime.UtcNow;
        client.UpdatedAt = DateTime.UtcNow;
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        
        return client;
    }

    public async Task<Client?> GetByIdAsync(Guid clientId, string tenantId)
    {
        return await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == clientId && c.TenantId == tenantId);
    }

    public async Task<Client?> UpdateAsync(Client client)
    {
        var existingClient = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == client.Id && c.TenantId == client.TenantId);
        
        if (existingClient == null)
            return null;
        
        existingClient.CompanyName = client.CompanyName;
        existingClient.Country = client.Country;
        existingClient.Address = client.Address;
        existingClient.IceNumber = client.IceNumber;
        existingClient.RcNumber = client.RcNumber;
        existingClient.VatNumber = client.VatNumber;
        existingClient.CnssNumber = client.CnssNumber;
        existingClient.Industry = client.Industry;
        existingClient.AdminContactPerson = client.AdminContactPerson;
        existingClient.BillingContactPerson = client.BillingContactPerson;
        existingClient.Status = client.Status;
        existingClient.FiscalYearEnd = client.FiscalYearEnd;
        existingClient.AssignedTeamId = client.AssignedTeamId;
        existingClient.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return existingClient;
    }

    public async Task<bool> DeleteAsync(Guid clientId, string tenantId, string? deletedBy = null)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Id == clientId && c.TenantId == tenantId);
        
        if (client == null)
            return false;
        
        client.IsDeleted = true;
        client.DeletedAt = DateTime.UtcNow;
        client.DeletedBy = deletedBy;
        client.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<(IList<Client> Items, int TotalCount)> ListAsync(
        string tenantId, 
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null)
    {
        var query = _context.Clients
            .Where(c => c.TenantId == tenantId);
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => 
                c.CompanyName.Contains(searchTerm) || 
                (c.IceNumber != null && c.IceNumber.Contains(searchTerm)) ||
                (c.RcNumber != null && c.RcNumber.Contains(searchTerm)) ||
                (c.VatNumber != null && c.VatNumber.Contains(searchTerm)) ||
                (c.Industry != null && c.Industry.Contains(searchTerm)));
        }
        
        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(c => c.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (items, totalCount);
    }

    public async Task<bool> IceNumberExistsAsync(string iceNumber, string tenantId, Guid? excludeClientId = null)
    {
        var query = _context.Clients
            .Where(c => c.TenantId == tenantId && c.IceNumber == iceNumber);
        
        if (excludeClientId.HasValue)
        {
            query = query.Where(c => c.Id != excludeClientId.Value);
        }
        
        return await query.AnyAsync();
    }
    
    public async Task<bool> RcNumberExistsAsync(string rcNumber, string tenantId, Guid? excludeClientId = null)
    {
        var query = _context.Clients
            .Where(c => c.TenantId == tenantId && c.RcNumber == rcNumber);
        
        if (excludeClientId.HasValue)
        {
            query = query.Where(c => c.Id != excludeClientId.Value);
        }
        
        return await query.AnyAsync();
    }
}