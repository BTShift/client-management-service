using Microsoft.Extensions.Logging;
using MassTransit;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;
using ClientManagement.Contract.Events;

namespace ClientManagement.Application.Services;

public class UserClientAssociationApplicationService : IUserClientAssociationApplicationService
{
    private readonly IUserClientAssociationRepository _userClientRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserClientAssociationApplicationService> _logger;

    public UserClientAssociationApplicationService(
        IUserClientAssociationRepository userClientRepository,
        IClientRepository clientRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<UserClientAssociationApplicationService> logger)
    {
        _userClientRepository = userClientRepository;
        _clientRepository = clientRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<UserClientAssociation> AssignUserToClientAsync(string userId, Guid clientId, string tenantId, string assignedBy)
    {
        _logger.LogInformation("Assigning user {UserId} to client {ClientId} in tenant {TenantId}", userId, clientId, tenantId);

        // Check if client exists
        var client = await _clientRepository.GetByIdAsync(clientId, tenantId);
        if (client == null)
        {
            throw new InvalidOperationException($"Client with ID {clientId} not found");
        }

        // Check if association already exists
        var existingAssociation = await _userClientRepository.GetAssociationAsync(userId, clientId, tenantId);
        if (existingAssociation != null)
        {
            _logger.LogWarning("User {UserId} is already assigned to client {ClientId}", userId, clientId);
            return existingAssociation;
        }

        // TODO: Validate user exists in Identity service
        var userExists = await _userClientRepository.UserExistsAsync(userId, tenantId);
        if (!userExists)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        // Create new association
        var association = new UserClientAssociation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ClientId = clientId,
            TenantId = tenantId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };

        var result = await _userClientRepository.AddAssociationAsync(association);

        // Publish event
        await _publishEndpoint.Publish(new UserAssignedToClientEvent
        {
            CorrelationId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ClientId = clientId.ToString(),
            UserId = userId,
            TenantId = tenantId,
            AssignedAt = association.AssignedAt,
            AssignedBy = assignedBy
        });

        _logger.LogInformation("Successfully assigned user {UserId} to client {ClientId}", userId, clientId);
        return result;
    }

    public async Task<bool> RemoveUserFromClientAsync(string userId, Guid clientId, string tenantId)
    {
        _logger.LogInformation("Removing user {UserId} from client {ClientId} in tenant {TenantId}", userId, clientId, tenantId);

        var result = await _userClientRepository.RemoveAssociationAsync(userId, clientId, tenantId);

        if (result)
        {
            // Publish event
            await _publishEndpoint.Publish(new UserRemovedFromClientEvent
            {
                CorrelationId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                ClientId = clientId.ToString(),
                UserId = userId,
                TenantId = tenantId
            });

            _logger.LogInformation("Successfully removed user {UserId} from client {ClientId}", userId, clientId);
        }
        else
        {
            _logger.LogWarning("User {UserId} was not assigned to client {ClientId}", userId, clientId);
        }

        return result;
    }

    public async Task<(List<UserClientAssociation> associations, int totalCount)> GetClientUsersAsync(Guid clientId, string tenantId, int page, int pageSize)
    {
        _logger.LogInformation("Getting users for client {ClientId} in tenant {TenantId}", clientId, tenantId);

        var associations = await _userClientRepository.GetClientUsersAsync(clientId, tenantId, page, pageSize);
        var totalCount = await _userClientRepository.GetClientUserCountAsync(clientId, tenantId);

        return (associations, totalCount);
    }

    public async Task<(List<UserClientAssociation> associations, int totalCount)> GetUserClientsAsync(string userId, string tenantId, int page, int pageSize)
    {
        _logger.LogInformation("Getting clients for user {UserId} in tenant {TenantId}", userId, tenantId);

        var associations = await _userClientRepository.GetUserClientsAsync(userId, tenantId, page, pageSize);
        var totalCount = await _userClientRepository.GetUserClientCountAsync(userId, tenantId);

        return (associations, totalCount);
    }
}