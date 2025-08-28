using Microsoft.Extensions.Logging;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;
using MassTransit;
using ClientManagement.Contract.Events;

namespace ClientManagement.Application.Services;

public interface IClientApplicationService
{
    Task<Client> CreateClientAsync(string tenantId, string companyName, string country, string address, 
        string iceNumber, string rcNumber, string vatNumber, string cnssNumber, 
        string industry, string adminContactPerson, string billingContactPerson, 
        string fiscalYearEnd, string assignedTeamId);
    Task<Client?> GetClientAsync(Guid clientId, string tenantId);
    Task<Client?> UpdateClientAsync(Guid clientId, string tenantId, string companyName, string country, 
        string address, string iceNumber, string rcNumber, string vatNumber, string cnssNumber, 
        string industry, string adminContactPerson, string billingContactPerson, 
        ClientStatus status, string fiscalYearEnd, string assignedTeamId);
    Task<bool> DeleteClientAsync(Guid clientId, string tenantId, string? deletedBy = null);
    Task<(IList<Client> Items, int TotalCount)> ListClientsAsync(string tenantId, int page, int pageSize, string? searchTerm = null);
}

public class ClientApplicationService : IClientApplicationService
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ClientApplicationService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserContext _userContext;

    public ClientApplicationService(
        IClientRepository clientRepository, 
        ILogger<ClientApplicationService> logger,
        IPublishEndpoint publishEndpoint,
        IUserContext userContext)
    {
        _clientRepository = clientRepository;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _userContext = userContext;
    }

    public async Task<Client> CreateClientAsync(string tenantId, string companyName, string country, string address, 
        string iceNumber, string rcNumber, string vatNumber, string cnssNumber, 
        string industry, string adminContactPerson, string billingContactPerson, 
        string fiscalYearEnd, string assignedTeamId)
    {
        _logger.LogInformation("Creating new client for tenant {TenantId}: {CompanyName}", tenantId, companyName);
        
        // Validate ICE number uniqueness within tenant if provided
        if (!string.IsNullOrWhiteSpace(iceNumber))
        {
            var iceExists = await _clientRepository.IceNumberExistsAsync(iceNumber, tenantId);
            if (iceExists)
            {
                _logger.LogWarning("Cannot create client: ICE number {IceNumber} already exists for tenant {TenantId}", iceNumber, tenantId);
                throw new InvalidOperationException($"ICE number '{iceNumber}' is already in use within this tenant.");
            }
        }
        
        // Validate RC number uniqueness within tenant if provided
        if (!string.IsNullOrWhiteSpace(rcNumber))
        {
            var rcExists = await _clientRepository.RcNumberExistsAsync(rcNumber, tenantId);
            if (rcExists)
            {
                _logger.LogWarning("Cannot create client: RC number {RcNumber} already exists for tenant {TenantId}", rcNumber, tenantId);
                throw new InvalidOperationException($"RC number '{rcNumber}' is already in use within this tenant.");
            }
        }
        
        var client = new Client
        {
            TenantId = tenantId,
            CompanyName = companyName,
            Country = string.IsNullOrWhiteSpace(country) ? null : country,
            Address = string.IsNullOrWhiteSpace(address) ? null : address,
            IceNumber = string.IsNullOrWhiteSpace(iceNumber) ? null : iceNumber,
            RcNumber = string.IsNullOrWhiteSpace(rcNumber) ? null : rcNumber,
            VatNumber = string.IsNullOrWhiteSpace(vatNumber) ? null : vatNumber,
            CnssNumber = string.IsNullOrWhiteSpace(cnssNumber) ? null : cnssNumber,
            Industry = string.IsNullOrWhiteSpace(industry) ? null : industry,
            AdminContactPerson = string.IsNullOrWhiteSpace(adminContactPerson) ? null : adminContactPerson,
            BillingContactPerson = string.IsNullOrWhiteSpace(billingContactPerson) ? null : billingContactPerson,
            Status = ClientStatus.Active,
            FiscalYearEnd = string.IsNullOrWhiteSpace(fiscalYearEnd) ? null : DateTime.Parse(fiscalYearEnd),
            AssignedTeamId = string.IsNullOrWhiteSpace(assignedTeamId) ? null : Guid.Parse(assignedTeamId)
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
            CompanyName = createdClient.CompanyName,
            Country = createdClient.Country,
            Address = createdClient.Address,
            IceNumber = createdClient.IceNumber,
            RcNumber = createdClient.RcNumber,
            VatNumber = createdClient.VatNumber,
            CnssNumber = createdClient.CnssNumber,
            Industry = createdClient.Industry,
            AdminContactPerson = createdClient.AdminContactPerson,
            BillingContactPerson = createdClient.BillingContactPerson,
            Status = createdClient.Status.ToString(),
            FiscalYearEnd = createdClient.FiscalYearEnd,
            AssignedTeamId = createdClient.AssignedTeamId?.ToString(),
            CreatedAt = createdClient.CreatedAt,
            CreatedBy = _userContext.IsAuthenticated() ? _userContext.GetCurrentUserName() : "System"
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

    public async Task<Client?> UpdateClientAsync(Guid clientId, string tenantId, string companyName, string country, 
        string address, string iceNumber, string rcNumber, string vatNumber, string cnssNumber, 
        string industry, string adminContactPerson, string billingContactPerson, 
        ClientStatus status, string fiscalYearEnd, string assignedTeamId)
    {
        _logger.LogInformation("Updating client {ClientId} for tenant {TenantId}", clientId, tenantId);
        
        // Validate ICE number uniqueness within tenant (excluding current client) if provided
        if (!string.IsNullOrWhiteSpace(iceNumber))
        {
            var iceExists = await _clientRepository.IceNumberExistsAsync(iceNumber, tenantId, clientId);
            if (iceExists)
            {
                _logger.LogWarning("Cannot update client: ICE number {IceNumber} already exists for tenant {TenantId}", iceNumber, tenantId);
                throw new InvalidOperationException($"ICE number '{iceNumber}' is already in use within this tenant.");
            }
        }
        
        // Validate RC number uniqueness within tenant (excluding current client) if provided
        if (!string.IsNullOrWhiteSpace(rcNumber))
        {
            var rcExists = await _clientRepository.RcNumberExistsAsync(rcNumber, tenantId, clientId);
            if (rcExists)
            {
                _logger.LogWarning("Cannot update client: RC number {RcNumber} already exists for tenant {TenantId}", rcNumber, tenantId);
                throw new InvalidOperationException($"RC number '{rcNumber}' is already in use within this tenant.");
            }
        }
        
        var client = new Client
        {
            Id = clientId,
            TenantId = tenantId,
            CompanyName = companyName,
            Country = string.IsNullOrWhiteSpace(country) ? null : country,
            Address = string.IsNullOrWhiteSpace(address) ? null : address,
            IceNumber = string.IsNullOrWhiteSpace(iceNumber) ? null : iceNumber,
            RcNumber = string.IsNullOrWhiteSpace(rcNumber) ? null : rcNumber,
            VatNumber = string.IsNullOrWhiteSpace(vatNumber) ? null : vatNumber,
            CnssNumber = string.IsNullOrWhiteSpace(cnssNumber) ? null : cnssNumber,
            Industry = string.IsNullOrWhiteSpace(industry) ? null : industry,
            AdminContactPerson = string.IsNullOrWhiteSpace(adminContactPerson) ? null : adminContactPerson,
            BillingContactPerson = string.IsNullOrWhiteSpace(billingContactPerson) ? null : billingContactPerson,
            Status = status,
            FiscalYearEnd = string.IsNullOrWhiteSpace(fiscalYearEnd) ? null : DateTime.Parse(fiscalYearEnd),
            AssignedTeamId = string.IsNullOrWhiteSpace(assignedTeamId) ? null : Guid.Parse(assignedTeamId)
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
                CompanyName = updatedClient.CompanyName,
                Country = updatedClient.Country,
                Address = updatedClient.Address,
                IceNumber = updatedClient.IceNumber,
                RcNumber = updatedClient.RcNumber,
                VatNumber = updatedClient.VatNumber,
                CnssNumber = updatedClient.CnssNumber,
                Industry = updatedClient.Industry,
                AdminContactPerson = updatedClient.AdminContactPerson,
                BillingContactPerson = updatedClient.BillingContactPerson,
                Status = updatedClient.Status.ToString(),
                FiscalYearEnd = updatedClient.FiscalYearEnd,
                AssignedTeamId = updatedClient.AssignedTeamId?.ToString(),
                UpdatedAt = updatedClient.UpdatedAt,
                UpdatedBy = _userContext.IsAuthenticated() ? _userContext.GetCurrentUserName() : "System"
            });
            
            _logger.LogInformation("Published ClientUpdatedEvent for client {ClientId} with correlation ID {CorrelationId}", 
                clientId, correlationId);
        }
        
        return updatedClient;
    }

    public async Task<bool> DeleteClientAsync(Guid clientId, string tenantId, string? deletedBy = null)
    {
        var actualDeletedBy = deletedBy ?? (_userContext.IsAuthenticated() ? _userContext.GetCurrentUserName() : "System");
        _logger.LogInformation("Soft deleting client {ClientId} for tenant {TenantId} by user {DeletedBy}", 
            clientId, tenantId, actualDeletedBy);
        
        var result = await _clientRepository.DeleteAsync(clientId, tenantId, actualDeletedBy);
        
        if (result)
        {
            // Enhanced audit logging for soft delete operations
            _logger.LogInformation(
                "AUDIT: Client soft delete successful - ClientId: {ClientId}, TenantId: {TenantId}, DeletedBy: {DeletedBy}, Timestamp: {Timestamp}",
                clientId, tenantId, actualDeletedBy, DateTime.UtcNow);
            
            // Publish ClientDeletedEvent
            var correlationId = Guid.NewGuid();
            await _publishEndpoint.Publish(new ClientDeletedEvent
            {
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                ClientId = clientId.ToString(),
                TenantId = tenantId,
                DeletedAt = DateTime.UtcNow,
                DeletedBy = actualDeletedBy
            });
            
            _logger.LogInformation("Published ClientDeletedEvent for client {ClientId} with correlation ID {CorrelationId}", 
                clientId, correlationId);
        }
        else
        {
            _logger.LogWarning(
                "AUDIT: Client soft delete failed (not found) - ClientId: {ClientId}, TenantId: {TenantId}, AttemptedBy: {DeletedBy}, Timestamp: {Timestamp}",
                clientId, tenantId, actualDeletedBy, DateTime.UtcNow);
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