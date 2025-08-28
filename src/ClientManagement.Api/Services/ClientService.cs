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
    private readonly IClientGroupApplicationService _clientGroupApplicationService;
    private readonly IUserContext<ServerCallContext> _userContext;

    public ClientService(
        ILogger<ClientService> logger, 
        IClientApplicationService clientApplicationService,
        IClientGroupApplicationService clientGroupApplicationService,
        IUserContext<ServerCallContext> userContext)
    {
        _logger = logger;
        _clientApplicationService = clientApplicationService;
        _clientGroupApplicationService = clientGroupApplicationService;
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
                request.Cif,
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
                request.Cif,
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
                    Cif = client.Cif,
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
            Cif = client.Cif,
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
    
    // Group Management Methods
    
    public override async Task<GroupResponse> CreateGroup(CreateGroupRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Creating group for tenant {TenantId}: {Name}", request.TenantId, request.Name);
            
            var tenantId = ExtractTenantId(request.TenantId, context);
            
            var group = await _clientGroupApplicationService.CreateGroupAsync(
                tenantId,
                request.Name,
                request.Description
            );
            
            return MapToGroupResponse(group);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating group");
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while creating the group"));
        }
    }
    
    public override async Task<GroupResponse> UpdateGroup(UpdateGroupRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Updating group {GroupId}", request.GroupId);
            
            if (!Guid.TryParse(request.GroupId, out var groupId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid group ID format"));
            }
            
            var tenantId = ExtractTenantId(null, context);
            
            var group = await _clientGroupApplicationService.UpdateGroupAsync(
                groupId,
                tenantId,
                request.Name,
                request.Description
            );
            
            if (group == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Group not found"));
            }
            
            return MapToGroupResponse(group);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating group");
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while updating the group"));
        }
    }
    
    public override async Task<DeleteGroupResponse> DeleteGroup(DeleteGroupRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Deleting group {GroupId}", request.GroupId);
            
            if (!Guid.TryParse(request.GroupId, out var groupId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid group ID format"));
            }
            
            var tenantId = ExtractTenantId(null, context);
            var userIdentity = _userContext.GetUserIdentity(context);
            
            var result = await _clientGroupApplicationService.DeleteGroupAsync(groupId, tenantId, userIdentity);
            
            return new DeleteGroupResponse
            {
                Success = result,
                Message = result ? "Group deleted successfully" : "Group not found"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while deleting the group"));
        }
    }
    
    public override async Task<ListGroupsResponse> ListGroups(ListGroupsRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Listing groups for tenant {TenantId}", request.TenantId);
            
            var tenantId = ExtractTenantId(request.TenantId, context);
            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = request.PageSize > 0 ? request.PageSize : 20;
            
            var (groups, totalCount) = await _clientGroupApplicationService.ListGroupsAsync(
                tenantId,
                page,
                pageSize,
                request.SearchTerm
            );
            
            var response = new ListGroupsResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            
            foreach (var group in groups)
            {
                response.Groups.Add(new GroupInfo
                {
                    GroupId = group.Id.ToString(),
                    Name = group.Name,
                    Description = group.Description ?? string.Empty,
                    ClientCount = group.ClientGroupMemberships?.Count ?? 0
                });
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing groups");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while listing groups"));
        }
    }
    
    public override async Task<AddClientToGroupResponse> AddClientToGroup(AddClientToGroupRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Adding client {ClientId} to group {GroupId}", request.ClientId, request.GroupId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            if (!Guid.TryParse(request.GroupId, out var groupId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid group ID format"));
            }
            
            var tenantId = ExtractTenantId(null, context);
            var userIdentity = _userContext.GetUserIdentity(context);
            
            var result = await _clientGroupApplicationService.AddClientToGroupAsync(
                clientId,
                groupId,
                tenantId,
                userIdentity
            );
            
            return new AddClientToGroupResponse
            {
                Success = result,
                Message = result ? "Client added to group successfully" : "Failed to add client to group"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding client to group");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while adding client to group"));
        }
    }
    
    public override async Task<RemoveClientFromGroupResponse> RemoveClientFromGroup(RemoveClientFromGroupRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Removing client {ClientId} from group {GroupId}", request.ClientId, request.GroupId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            if (!Guid.TryParse(request.GroupId, out var groupId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid group ID format"));
            }
            
            var tenantId = ExtractTenantId(null, context);
            
            var result = await _clientGroupApplicationService.RemoveClientFromGroupAsync(clientId, groupId, tenantId);
            
            return new RemoveClientFromGroupResponse
            {
                Success = result,
                Message = result ? "Client removed from group successfully" : "Failed to remove client from group"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing client from group");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while removing client from group"));
        }
    }
    
    public override async Task<GetGroupClientsResponse> GetGroupClients(GetGroupClientsRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Getting clients for group {GroupId}", request.GroupId);
            
            if (!Guid.TryParse(request.GroupId, out var groupId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid group ID format"));
            }
            
            var tenantId = ExtractTenantId(request.TenantId, context);
            
            var clients = await _clientGroupApplicationService.GetGroupClientsAsync(groupId, tenantId);
            
            var response = new GetGroupClientsResponse
            {
                TotalCount = clients.Count
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
            _logger.LogError(ex, "Error getting group clients");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while getting group clients"));
        }
    }
    
    public override async Task<GetClientGroupsResponse> GetClientGroups(GetClientGroupsRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Getting groups for client {ClientId}", request.ClientId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            var tenantId = ExtractTenantId(request.TenantId, context);
            
            var groups = await _clientGroupApplicationService.GetClientGroupsAsync(clientId, tenantId);
            
            var response = new GetClientGroupsResponse
            {
                TotalCount = groups.Count
            };
            
            foreach (var group in groups)
            {
                response.Groups.Add(new GroupInfo
                {
                    GroupId = group.Id.ToString(),
                    Name = group.Name,
                    Description = group.Description ?? string.Empty,
                    ClientCount = group.ClientGroupMemberships?.Count ?? 0
                });
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client groups");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while getting client groups"));
        }
    }
    
    private static GroupResponse MapToGroupResponse(ClientGroup group)
    {
        return new GroupResponse
        {
            GroupId = group.Id.ToString(),
            Name = group.Name,
            Description = group.Description ?? string.Empty,
            TenantId = group.TenantId,
            CreatedAt = group.CreatedAt.ToString("O"),
            UpdatedAt = group.UpdatedAt.ToString("O"),
            ClientCount = group.ClientGroupMemberships?.Count ?? 0
        };
    }
}