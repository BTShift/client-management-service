using Microsoft.Extensions.Logging;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;
using MassTransit;
using ClientManagement.Contract.Events;

namespace ClientManagement.Application.Services;

public interface IClientApplicationService
{
    Task<Client> CreateClientAsync(string tenantId, string name, string cif, string email, string phone, string address);
    Task<Client?> GetClientAsync(Guid clientId, string tenantId);
    Task<Client?> UpdateClientAsync(Guid clientId, string tenantId, string name, string cif, string email, string phone, string address, ClientStatus status);
    Task<bool> DeleteClientAsync(Guid clientId, string tenantId, string? deletedBy = null);
    Task<(IList<Client> Items, int TotalCount)> ListClientsAsync(string tenantId, int page, int pageSize, string? searchTerm = null);
}

public class ClientApplicationService : IClientApplicationService
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ClientApplicationService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public ClientApplicationService(
        IClientRepository clientRepository, 
        ILogger<ClientApplicationService> logger,
        IPublishEndpoint publishEndpoint)
    {
        _clientRepository = clientRepository;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Client> CreateClientAsync(string tenantId, string name, string cif, string email, string phone, string address)
    {
        _logger.LogInformation("Creating new client for tenant {TenantId}: {Name}", tenantId, name);
        
        // Validate email uniqueness within tenant
        var emailExists = await _clientRepository.EmailExistsAsync(email, tenantId);
        if (emailExists)
        {
            _logger.LogWarning("Cannot create client: Email {Email} already exists for tenant {TenantId}", email, tenantId);
            throw new InvalidOperationException($"Email '{email}' is already in use within this tenant.");
        }
        
        // Validate CIF uniqueness within tenant
        var cifExists = await _clientRepository.CifExistsAsync(cif, tenantId);
        if (cifExists)
        {
            _logger.LogWarning("Cannot create client: CIF {Cif} already exists for tenant {TenantId}", cif, tenantId);
            throw new InvalidOperationException($"CIF '{cif}' is already in use within this tenant.");
        }
        
        var client = new Client
        {
            TenantId = tenantId,
            Name = name,
            Cif = cif,
            Email = email,
            Phone = phone,
            Address = address,
            Status = ClientStatus.Active
        };
        
        var createdClient = await _clientRepository.CreateAsync(client);
        
        _logger.LogInformation("Client created successfully with ID {ClientId} for tenant {TenantId}", 
            createdClient.Id, tenantId);
        
        // Publish ClientCreatedEvent
        var correlationId = Guid.NewGuid();
        await _publishEndpoint.Publish(new ClientCreatedEvent
        {
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            ClientId = createdClient.Id.ToString(),
            TenantId = tenantId,
            Name = createdClient.Name,
            Cif = createdClient.Cif,
            Email = createdClient.Email,
            Phone = createdClient.Phone,
            Address = createdClient.Address,
            Status = createdClient.Status.ToString(),
            CreatedAt = createdClient.CreatedAt,
            CreatedBy = "System" // TODO: Get from IUserContext when implemented
        });
        
        _logger.LogInformation("Published ClientCreatedEvent for client {ClientId} with correlation ID {CorrelationId}", 
            createdClient.Id, correlationId);
        
        return createdClient;
    }

    public async Task<Client?> GetClientAsync(Guid clientId, string tenantId)
    {
        _logger.LogInformation("Retrieving client {ClientId} for tenant {TenantId}", clientId, tenantId);
        
        var client = await _clientRepository.GetByIdAsync(clientId, tenantId);
        
        if (client == null)
        {
            _logger.LogWarning("Client {ClientId} not found for tenant {TenantId}", clientId, tenantId);
        }
        
        return client;
    }

    public async Task<Client?> UpdateClientAsync(Guid clientId, string tenantId, string name, string cif,
        string email, string phone, string address, ClientStatus status)
    {
        _logger.LogInformation("Updating client {ClientId} for tenant {TenantId}", clientId, tenantId);
        
        // Validate email uniqueness within tenant (excluding current client)
        var emailExists = await _clientRepository.EmailExistsAsync(email, tenantId, clientId);
        if (emailExists)
        {
            _logger.LogWarning("Cannot update client: Email {Email} already exists for tenant {TenantId}", email, tenantId);
            throw new InvalidOperationException($"Email '{email}' is already in use within this tenant.");
        }
        
        // Validate CIF uniqueness within tenant (excluding current client)
        var cifExists = await _clientRepository.CifExistsAsync(cif, tenantId, clientId);
        if (cifExists)
        {
            _logger.LogWarning("Cannot update client: CIF {Cif} already exists for tenant {TenantId}", cif, tenantId);
            throw new InvalidOperationException($"CIF '{cif}' is already in use within this tenant.");
        }
        
        var client = new Client
        {
            Id = clientId,
            TenantId = tenantId,
            Name = name,
            Cif = cif,
            Email = email,
            Phone = phone,
            Address = address,
            Status = status
        };
        
        var updatedClient = await _clientRepository.UpdateAsync(client);
        
        if (updatedClient == null)
        {
            _logger.LogWarning("Client {ClientId} not found for update in tenant {TenantId}", clientId, tenantId);
        }
        else
        {
            _logger.LogInformation("Client {ClientId} updated successfully for tenant {TenantId}", clientId, tenantId);
            
            // Publish ClientUpdatedEvent
            var correlationId = Guid.NewGuid();
            await _publishEndpoint.Publish(new ClientUpdatedEvent
            {
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                ClientId = updatedClient.Id.ToString(),
                TenantId = tenantId,
                Name = updatedClient.Name,
                Cif = updatedClient.Cif,
                Email = updatedClient.Email,
                Phone = updatedClient.Phone,
                Address = updatedClient.Address,
                Status = updatedClient.Status.ToString(),
                UpdatedAt = updatedClient.UpdatedAt,
                UpdatedBy = "System" // TODO: Get from IUserContext when implemented
            });
            
            _logger.LogInformation("Published ClientUpdatedEvent for client {ClientId} with correlation ID {CorrelationId}", 
                clientId, correlationId);
        }
        
        return updatedClient;
    }

    public async Task<bool> DeleteClientAsync(Guid clientId, string tenantId, string? deletedBy = null)
    {
        _logger.LogInformation("Soft deleting client {ClientId} for tenant {TenantId} by user {DeletedBy}", 
            clientId, tenantId, deletedBy ?? "Unknown");
        
        var result = await _clientRepository.DeleteAsync(clientId, tenantId, deletedBy);
        
        if (result)
        {
            // Enhanced audit logging for soft delete operations
            _logger.LogInformation(
                "AUDIT: Client soft delete successful - ClientId: {ClientId}, TenantId: {TenantId}, DeletedBy: {DeletedBy}, Timestamp: {Timestamp}",
                clientId, tenantId, deletedBy ?? "Unknown", DateTime.UtcNow);
            
            // Publish ClientDeletedEvent
            var correlationId = Guid.NewGuid();
            await _publishEndpoint.Publish(new ClientDeletedEvent
            {
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                ClientId = clientId.ToString(),
                TenantId = tenantId,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = deletedBy ?? "System"
            });
            
            _logger.LogInformation("Published ClientDeletedEvent for client {ClientId} with correlation ID {CorrelationId}", 
                clientId, correlationId);
        }
        else
        {
            _logger.LogWarning(
                "AUDIT: Client soft delete failed (not found) - ClientId: {ClientId}, TenantId: {TenantId}, AttemptedBy: {DeletedBy}, Timestamp: {Timestamp}",
                clientId, tenantId, deletedBy ?? "Unknown", DateTime.UtcNow);
        }
        
        return result;
    }

    public async Task<(IList<Client> Items, int TotalCount)> ListClientsAsync(string tenantId, int page, int pageSize, string? searchTerm = null)
    {
        _logger.LogInformation("Listing clients for tenant {TenantId} - Page: {Page}, PageSize: {PageSize}", 
            tenantId, page, pageSize);
        
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Limit max page size
        
        return await _clientRepository.ListAsync(tenantId, page, pageSize, searchTerm);
    }
}