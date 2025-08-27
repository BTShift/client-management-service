using Grpc.Core;
using Microsoft.Extensions.Logging;
using ClientManagement.Contract;
using ClientManagement.Application.Services;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;

namespace ClientManagement.Api.Services;

public class ClientService : Contract.ClientManagement.ClientManagementBase
{
    private readonly ILogger<ClientService> _logger;
    private readonly IClientApplicationService _clientApplicationService;
    private readonly IUserContext<ServerCallContext> _userContext;

    public ClientService(
        ILogger<ClientService> logger, 
        IClientApplicationService clientApplicationService,
        IUserContext<ServerCallContext> userContext)
    {
        _logger = logger;
        _clientApplicationService = clientApplicationService;
        _userContext = userContext;
    }

    public override async Task<ClientResponse> CreateClient(CreateClientRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Creating client for tenant {TenantId}: {Name}", request.TenantId, request.Name);
            
            // Extract tenant ID from request or context
            var tenantId = ExtractTenantId(request.TenantId, context);
            
            var client = await _clientApplicationService.CreateClientAsync(
                tenantId,
                request.Name,
                request.Email,
                request.Phone ?? string.Empty,
                request.Address ?? string.Empty
            );
            
            return MapToClientResponse(client);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating client");
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the client"));
        }
    }

    public override async Task<ClientResponse> GetClient(ClientRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Getting client with ID: {ClientId}", request.ClientId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            // Extract tenant ID from context
            var tenantId = ExtractTenantId(null, context);
            
            var client = await _clientApplicationService.GetClientAsync(clientId, tenantId);
            
            if (client == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Client with ID {request.ClientId} not found"));
            }
            
            return MapToClientResponse(client);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving client");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while retrieving the client"));
        }
    }

    public override async Task<ClientResponse> UpdateClient(UpdateClientRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Updating client with ID: {ClientId}", request.ClientId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            // Extract tenant ID from context
            var tenantId = ExtractTenantId(null, context);
            
            // Parse status
            if (!Enum.TryParse<ClientStatus>(request.Status ?? "Active", out var status))
            {
                status = ClientStatus.Active;
            }
            
            var client = await _clientApplicationService.UpdateClientAsync(
                clientId,
                tenantId,
                request.Name,
                request.Email,
                request.Phone ?? string.Empty,
                request.Address ?? string.Empty,
                status
            );
            
            if (client == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Client with ID {request.ClientId} not found"));
            }
            
            return MapToClientResponse(client);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating client");
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating client");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the client"));
        }
    }

    public override async Task<DeleteClientResponse> DeleteClient(DeleteClientRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Deleting client with ID: {ClientId}", request.ClientId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            // Extract tenant ID and user information from context
            var tenantId = ExtractTenantId(null, context);
            var deletedBy = _userContext.GetUserIdentity(context);
            
            var result = await _clientApplicationService.DeleteClientAsync(clientId, tenantId, deletedBy);
            
            return new DeleteClientResponse
            {
                Success = result,
                Message = result ? "Client deleted successfully" : "Client not found"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting client");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the client"));
        }
    }

    public override async Task<ClientListResponse> ListClients(ClientListRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Listing clients for tenant: {TenantId}, Page: {Page}, PageSize: {PageSize}", 
                request.TenantId, request.Page, request.PageSize);
            
            // Extract tenant ID from request or context
            var tenantId = ExtractTenantId(request.TenantId, context);
            
            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 20;
            
            var (clients, totalCount) = await _clientApplicationService.ListClientsAsync(
                tenantId, 
                page, 
                pageSize
            );
            
            var response = new ClientListResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            
            foreach (var client in clients)
            {
                response.Clients.Add(new ClientInfo
                {
                    ClientId = client.Id.ToString(),
                    Name = client.Name,
                    Email = client.Email,
                    Status = client.Status.ToString()
                });
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing clients");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while listing clients"));
        }
    }

    private static ClientResponse MapToClientResponse(Client client)
    {
        return new ClientResponse
        {
            ClientId = client.Id.ToString(),
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            Status = client.Status.ToString(),
            TenantId = client.TenantId,
            CreatedAt = client.CreatedAt.ToString("O"),
            UpdatedAt = client.UpdatedAt.ToString("O")
        };
    }

    private string ExtractTenantId(string? requestTenantId, ServerCallContext context)
    {
        // First try to get from request if provided
        if (!string.IsNullOrEmpty(requestTenantId))
        {
            return requestTenantId;
        }
        
        // Try to get from metadata/headers
        var tenantIdEntry = context.RequestHeaders.GetValue("x-tenant-id");
        if (!string.IsNullOrEmpty(tenantIdEntry))
        {
            return tenantIdEntry;
        }
        
        // For development/testing, use a default tenant
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            return "default-tenant";
        }
        
        throw new RpcException(new Status(StatusCode.Unauthenticated, "Tenant ID not provided"));
    }
}