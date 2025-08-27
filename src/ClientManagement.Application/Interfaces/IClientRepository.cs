using ClientManagement.Domain.Entities;

namespace ClientManagement.Application.Interfaces;

public interface IClientRepository
{
    Task<Client> CreateAsync(Client client);
    Task<Client?> GetByIdAsync(Guid clientId, string tenantId);
    Task<Client?> UpdateAsync(Client client);
    Task<bool> DeleteAsync(Guid clientId, string tenantId, string? deletedBy = null);
    Task<(IList<Client> Items, int TotalCount)> ListAsync(
        string tenantId, 
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null);
    Task<bool> EmailExistsAsync(string email, string tenantId, Guid? excludeClientId = null);
    Task<bool> CifExistsAsync(string cif, string tenantId, Guid? excludeClientId = null);
}