using Xunit;
using Moq;
using FluentAssertions;
using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging;
using ClientManagement.Api.Services;
using ClientManagement.Application.Services;
using ClientManagement.Application.Interfaces;
using ClientManagement.Contract;
using ClientManagement.Domain.Entities;

namespace ClientManagement.UnitTests.Api;

public class ClientServiceTests
{
    private readonly Mock<ILogger<ClientService>> _mockLogger;
    private readonly Mock<IClientApplicationService> _mockApplicationService;
    private readonly Mock<IClientGroupApplicationService> _mockGroupApplicationService;
    private readonly Mock<IUserContext<ServerCallContext>> _mockUserContext;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _mockLogger = new Mock<ILogger<ClientService>>();
        _mockApplicationService = new Mock<IClientApplicationService>();
        _mockGroupApplicationService = new Mock<IClientGroupApplicationService>();
        _mockUserContext = new Mock<IUserContext<ServerCallContext>>();
        _service = new ClientService(_mockLogger.Object, _mockApplicationService.Object, _mockGroupApplicationService.Object, _mockUserContext.Object);
    }
    
    private static ServerCallContext CreateTestContext(string? tenantId = null)
    {
        var metadata = new Metadata();
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            metadata.Add("x-tenant-id", tenantId);
        }
        
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        
        var testContext = TestServerCallContext.Create(
            "TestMethod",
            "localhost",
            DateTime.UtcNow.AddMinutes(1),
            metadata,
            CancellationToken.None,
            "127.0.0.1",
            null,
            null,
            m => Task.CompletedTask,
            () => new WriteOptions(),
            wo => { });
            
        return testContext;
    }

    [Fact]
    public async Task CreateClient_ShouldReturnClientResponse_WhenSuccessful()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            TenantId = "tenant-123",
            Name = "John Doe",
            Cif = "CIF123456",
            Email = "john@example.com",
            Phone = "123-456-7890",
            Address = "123 Main St"
        };
        
        var createdClient = new Client
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            Cif = request.Cif,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockApplicationService
            .Setup(x => x.CreateClientAsync(
                request.TenantId,
                request.Name,
                request.Cif,
                request.Email,
                request.Phone,
                request.Address))
            .ReturnsAsync(createdClient);

        // Act
        var result = await _service.CreateClient(request, CreateTestContext());

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(createdClient.Id.ToString());
        result.Name.Should().Be(createdClient.Name);
        result.Cif.Should().Be(createdClient.Cif);
        result.Email.Should().Be(createdClient.Email);
        result.TenantId.Should().Be(createdClient.TenantId);
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task CreateClient_ShouldThrowRpcException_WhenEmailExists()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            TenantId = "tenant-123",
            Name = "John Doe",
            Email = "existing@example.com"
        };
        
        _mockApplicationService
            .Setup(x => x.CreateClientAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _service.CreateClient(request, CreateTestContext()));
        
        exception.Status.StatusCode.Should().Be(StatusCode.AlreadyExists);
    }

    [Fact]
    public async Task GetClient_ShouldReturnClient_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new ClientRequest { ClientId = clientId.ToString() };
        
        var client = new Client
        {
            Id = clientId,
            TenantId = "tenant-123",
            Name = "John Doe",
            Cif = "CIF456",
            Email = "john@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockApplicationService
            .Setup(x => x.GetClientAsync(clientId, "tenant-123"))
            .ReturnsAsync(client);

        // Act
        var result = await _service.GetClient(request, CreateTestContext("tenant-123"));

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(clientId.ToString());
        result.Name.Should().Be(client.Name);
        result.Email.Should().Be(client.Email);
    }

    [Fact]
    public async Task GetClient_ShouldThrowNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new ClientRequest { ClientId = clientId.ToString() };
        
        _mockApplicationService
            .Setup(x => x.GetClientAsync(clientId, "tenant-123"))
            .ReturnsAsync((Client?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _service.GetClient(request, CreateTestContext("tenant-123")));
        
        exception.Status.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task GetClient_ShouldThrowInvalidArgument_WhenClientIdIsInvalid()
    {
        // Arrange
        var request = new ClientRequest { ClientId = "invalid-guid" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _service.GetClient(request, CreateTestContext()));
        
        exception.Status.StatusCode.Should().Be(StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task UpdateClient_ShouldReturnUpdatedClient_WhenSuccessful()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new UpdateClientRequest
        {
            ClientId = clientId.ToString(),
            Name = "Updated Name",
            Cif = "CIF789",
            Email = "updated@example.com",
            Phone = "999-888-7777",
            Address = "456 New St",
            Status = "Inactive"
        };
        
        var updatedClient = new Client
        {
            Id = clientId,
            TenantId = "tenant-123",
            Name = request.Name,
            Cif = request.Cif,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Status = ClientStatus.Inactive,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockApplicationService
            .Setup(x => x.UpdateClientAsync(
                clientId,
                "tenant-123",
                request.Name,
                request.Cif,
                request.Email,
                request.Phone,
                request.Address,
                ClientStatus.Inactive))
            .ReturnsAsync(updatedClient);

        // Act
        var result = await _service.UpdateClient(request, CreateTestContext("tenant-123"));

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(clientId.ToString());
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email);
        result.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task DeleteClient_ShouldReturnSuccess_WhenClientDeleted()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new DeleteClientRequest { ClientId = clientId.ToString() };
        var deletedBy = "test-user@example.com";
        
        _mockUserContext
            .Setup(x => x.GetUserIdentity(It.IsAny<ServerCallContext>()))
            .Returns(deletedBy);
        
        _mockApplicationService
            .Setup(x => x.DeleteClientAsync(clientId, "tenant-123", deletedBy))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteClient(request, CreateTestContext("tenant-123"));

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Client deleted successfully");
    }

    [Fact]
    public async Task DeleteClient_ShouldReturnFailure_WhenClientNotFound()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new DeleteClientRequest { ClientId = clientId.ToString() };
        
        _mockUserContext
            .Setup(x => x.GetUserIdentity(It.IsAny<ServerCallContext>()))
            .Returns((string?)null);
        
        _mockApplicationService
            .Setup(x => x.DeleteClientAsync(clientId, "tenant-123", null))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteClient(request, CreateTestContext("tenant-123"));

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Client not found");
    }

    [Fact]
    public async Task ListClients_ShouldReturnPagedResults()
    {
        // Arrange
        var request = new ClientListRequest
        {
            TenantId = "tenant-123",
            Page = 1,
            PageSize = 10
        };
        
        var clients = new List<Client>
        {
            new Client { Id = Guid.NewGuid(), Name = "Client 1", Email = "client1@example.com", Status = ClientStatus.Active },
            new Client { Id = Guid.NewGuid(), Name = "Client 2", Email = "client2@example.com", Status = ClientStatus.Inactive }
        };
        
        _mockApplicationService
            .Setup(x => x.ListClientsAsync("tenant-123", 1, 10, null))
            .ReturnsAsync((clients, 2));

        // Act
        var result = await _service.ListClients(request, CreateTestContext());

        // Assert
        result.Should().NotBeNull();
        result.Clients.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Clients[0].Name.Should().Be("Client 1");
        result.Clients[1].Name.Should().Be("Client 2");
    }

    [Fact]
    public async Task ExtractTenantId_ShouldUseRequestTenantId_WhenProvided()
    {
        // Arrange
        var request = new ClientListRequest
        {
            TenantId = "request-tenant",
            Page = 1,
            PageSize = 10
        };
        
        _mockApplicationService
            .Setup(x => x.ListClientsAsync("request-tenant", 1, 10, null))
            .ReturnsAsync((new List<Client>(), 0));

        // Act
        await _service.ListClients(request, CreateTestContext());

        // Assert
        _mockApplicationService.Verify(x => x.ListClientsAsync("request-tenant", 1, 10, null), Times.Once);
    }

    [Fact]
    public async Task ExtractTenantId_ShouldUseHeaderTenantId_WhenRequestTenantIdIsEmpty()
    {
        // Arrange
        var request = new ClientListRequest
        {
            TenantId = "",
            Page = 1,
            PageSize = 10
        };
        
        _mockApplicationService
            .Setup(x => x.ListClientsAsync("header-tenant", 1, 10, null))
            .ReturnsAsync((new List<Client>(), 0));

        // Act
        await _service.ListClients(request, CreateTestContext("header-tenant"));

        // Assert
        _mockApplicationService.Verify(x => x.ListClientsAsync("header-tenant", 1, 10, null), Times.Once);
    }
}