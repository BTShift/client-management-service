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
    private readonly IUserClientAssociationApplicationService _userClientAssociationService;
    private readonly IUserContext _userContext;

    public ClientService(
        ILogger<ClientService> logger, 
        IClientApplicationService clientApplicationService,
        IClientGroupApplicationService clientGroupApplicationService,
        IUserClientAssociationApplicationService userClientAssociationService,
        IUserContext userContext)
    {
        _logger = logger;
        _clientApplicationService = clientApplicationService;
        _clientGroupApplicationService = clientGroupApplicationService;
        _userClientAssociationService = userClientAssociationService;
        _userContext = userContext;
    }

    public override async Task<ClientResponse> CreateClient(CreateClientRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Creating client for tenant {TenantId}: {CompanyName}", request.TenantId, request.CompanyName);
            
            // Extract tenant ID from request or context
            var tenantId = ExtractTenantId(request.TenantId, context);
            
            var client = await _clientApplicationService.CreateClientAsync(
                tenantId,
                request.CompanyName,
                request.Country ?? string.Empty,
                request.Address ?? string.Empty,
                request.IceNumber ?? string.Empty,
                request.RcNumber ?? string.Empty,
                request.VatNumber ?? string.Empty,
                request.CnssNumber ?? string.Empty,
                request.Industry ?? string.Empty,
                request.AdminContactPerson ?? string.Empty,
                request.BillingContactPerson ?? string.Empty,
                request.FiscalYearEnd ?? string.Empty,
                request.AssignedTeamId ?? string.Empty
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
                request.CompanyName,
                request.Country ?? string.Empty,
                request.Address ?? string.Empty,
                request.IceNumber ?? string.Empty,
                request.RcNumber ?? string.Empty,
                request.VatNumber ?? string.Empty,
                request.CnssNumber ?? string.Empty,
                request.Industry ?? string.Empty,
                request.AdminContactPerson ?? string.Empty,
                request.BillingContactPerson ?? string.Empty,
                status,
                request.FiscalYearEnd ?? string.Empty,
                request.AssignedTeamId ?? string.Empty
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
            var deletedBy = _userContext.GetCurrentUserName();
            
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
                    CompanyName = client.CompanyName,
                    IceNumber = client.IceNumber ?? string.Empty,
                    RcNumber = client.RcNumber ?? string.Empty,
                    VatNumber = client.VatNumber ?? string.Empty,
                    CnssNumber = client.CnssNumber ?? string.Empty,
                    Industry = client.Industry ?? string.Empty,
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
            CompanyName = client.CompanyName,
            Country = client.Country ?? string.Empty,
            Address = client.Address ?? string.Empty,
            IceNumber = client.IceNumber ?? string.Empty,
            RcNumber = client.RcNumber ?? string.Empty,
            VatNumber = client.VatNumber ?? string.Empty,
            CnssNumber = client.CnssNumber ?? string.Empty,
            Industry = client.Industry ?? string.Empty,
            AdminContactPerson = client.AdminContactPerson ?? string.Empty,
            BillingContactPerson = client.BillingContactPerson ?? string.Empty,
            Status = client.Status.ToString(),
            TenantId = client.TenantId,
            FiscalYearEnd = client.FiscalYearEnd?.ToString("yyyy-MM-dd") ?? string.Empty,
            AssignedTeamId = client.AssignedTeamId?.ToString() ?? string.Empty,
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
            var userIdentity = _userContext.GetCurrentUserName();
            
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
            var userIdentity = _userContext.GetCurrentUserName();
            
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
                    CompanyName = client.CompanyName,
                    IceNumber = client.IceNumber ?? string.Empty,
                    RcNumber = client.RcNumber ?? string.Empty,
                    VatNumber = client.VatNumber ?? string.Empty,
                    CnssNumber = client.CnssNumber ?? string.Empty,
                    Industry = client.Industry ?? string.Empty,
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
    
    // User-Client Association Methods
    
    public override async Task<AssignUserToClientResponse> AssignUserToClient(AssignUserToClientRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Assigning user {UserId} to client {ClientId}", request.UserId, request.ClientId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            var tenantId = ExtractTenantId(request.TenantId, context);
            var assignedBy = request.AssignedBy ?? _userContext.GetCurrentUserName() ?? "system";
            
            var association = await _userClientAssociationService.AssignUserToClientAsync(
                request.UserId,
                clientId,
                tenantId,
                assignedBy
            );
            
            return new AssignUserToClientResponse
            {
                Success = true,
                Message = $"User {request.UserId} successfully assigned to client {request.ClientId}",
                AssociationId = association.Id.ToString()
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while assigning user to client");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user to client");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while assigning user to client"));
        }
    }
    
    public override async Task<RemoveUserFromClientResponse> RemoveUserFromClient(RemoveUserFromClientRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Removing user {UserId} from client {ClientId}", request.UserId, request.ClientId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            var tenantId = ExtractTenantId(request.TenantId, context);
            
            var success = await _userClientAssociationService.RemoveUserFromClientAsync(
                request.UserId,
                clientId,
                tenantId
            );
            
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} is not assigned to client {request.ClientId}"));
            }
            
            return new RemoveUserFromClientResponse
            {
                Success = true,
                Message = $"User {request.UserId} successfully removed from client {request.ClientId}"
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user from client");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while removing user from client"));
        }
    }
    
    public override async Task<GetClientUsersResponse> GetClientUsers(GetClientUsersRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Getting users for client {ClientId}", request.ClientId);
            
            if (!Guid.TryParse(request.ClientId, out var clientId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid client ID format"));
            }
            
            var tenantId = ExtractTenantId(request.TenantId, context);
            var page = Math.Max(1, request.Page);
            var pageSize = request.PageSize > 0 ? request.PageSize : 20;
            
            var (associations, totalCount) = await _userClientAssociationService.GetClientUsersAsync(
                clientId,
                tenantId,
                page,
                pageSize
            );
            
            var response = new GetClientUsersResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            
            foreach (var association in associations)
            {
                response.Users.Add(new UserInfo
                {
                    UserId = association.UserId,
                    Email = "", // TODO: Fetch from Identity service
                    FirstName = "", // TODO: Fetch from Identity service
                    LastName = "", // TODO: Fetch from Identity service
                    AssignedAt = association.AssignedAt.ToString("O"),
                    AssignedBy = association.AssignedBy
                });
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client users");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while getting client users"));
        }
    }
    
    public override async Task<GetUserClientsResponse> GetUserClients(GetUserClientsRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Getting clients for user {UserId}", request.UserId);
            
            var tenantId = ExtractTenantId(request.TenantId, context);
            var page = Math.Max(1, request.Page);
            var pageSize = request.PageSize > 0 ? request.PageSize : 20;
            
            var (associations, totalCount) = await _userClientAssociationService.GetUserClientsAsync(
                request.UserId,
                tenantId,
                page,
                pageSize
            );
            
            var response = new GetUserClientsResponse
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            
            foreach (var association in associations)
            {
                if (association.Client != null)
                {
                    response.Clients.Add(new ClientInfo
                    {
                        ClientId = association.Client.Id.ToString(),
                        CompanyName = association.Client.CompanyName,
                        IceNumber = association.Client.IceNumber ?? string.Empty,
                        RcNumber = association.Client.RcNumber ?? string.Empty,
                        VatNumber = association.Client.VatNumber ?? string.Empty,
                        CnssNumber = association.Client.CnssNumber ?? string.Empty,
                        Industry = association.Client.Industry ?? string.Empty,
                        Status = association.Client.Status.ToString()
                    });
                }
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user clients");
            throw new RpcException(new Status(StatusCode.Internal, "An error occurred while getting user clients"));
        }
    }
}