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
    private readonly Mock<IUserClientAssociationApplicationService> _mockUserClientAssociationService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _mockLogger = new Mock<ILogger<ClientService>>();
        _mockApplicationService = new Mock<IClientApplicationService>();
        _mockGroupApplicationService = new Mock<IClientGroupApplicationService>();
        _mockUserClientAssociationService = new Mock<IUserClientAssociationApplicationService>();
        _mockUserContext = new Mock<IUserContext>();
        
        // Setup default user context behavior
        _mockUserContext.Setup(x => x.IsAuthenticated()).Returns(true);
        _mockUserContext.Setup(x => x.GetCurrentUserName()).Returns("test-user");
        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns("user-123");
        _mockUserContext.Setup(x => x.GetCurrentTenantId()).Returns("tenant-123");
        
        _service = new ClientService(
            _mockLogger.Object, 
            _mockApplicationService.Object, 
            _mockGroupApplicationService.Object, 
            _mockUserClientAssociationService.Object,
            _mockUserContext.Object);
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
            CompanyName = "Test Company Ltd",
            Country = "Morocco",
            Address = "123 Main St, Casablanca",
            IceNumber = "ICE123456789",
            RcNumber = "RC12345",
            VatNumber = "VAT12345",
            CnssNumber = "CNSS12345",
            Industry = "Technology",
            AdminContactPerson = "John Doe",
            BillingContactPerson = "Jane Doe",
            FiscalYearEnd = "2024-12-31",
            AssignedTeamId = ""
        };
        
        var createdClient = new Client
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            CompanyName = request.CompanyName,
            Country = request.Country,
            Address = request.Address,
            IceNumber = request.IceNumber,
            RcNumber = request.RcNumber,
            VatNumber = request.VatNumber,
            CnssNumber = request.CnssNumber,
            Industry = request.Industry,
            AdminContactPerson = request.AdminContactPerson,
            BillingContactPerson = request.BillingContactPerson,
            Status = ClientStatus.Active,
            FiscalYearEnd = DateTime.Parse(request.FiscalYearEnd),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockApplicationService
            .Setup(x => x.CreateClientAsync(
                request.TenantId,
                request.CompanyName,
                request.Country,
                request.Address,
                request.IceNumber,
                request.RcNumber,
                request.VatNumber,
                request.CnssNumber,
                request.Industry,
                request.AdminContactPerson,
                request.BillingContactPerson,
                request.FiscalYearEnd,
                request.AssignedTeamId))
            .ReturnsAsync(createdClient);

        // Act
        var result = await _service.CreateClient(request, CreateTestContext());

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(createdClient.Id.ToString());
        result.CompanyName.Should().Be(createdClient.CompanyName);
        result.IceNumber.Should().Be(createdClient.IceNumber);
        result.RcNumber.Should().Be(createdClient.RcNumber);
        result.VatNumber.Should().Be(createdClient.VatNumber);
        result.CnssNumber.Should().Be(createdClient.CnssNumber);
        result.TenantId.Should().Be(createdClient.TenantId);
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task CreateClient_ShouldThrowRpcException_WhenIceNumberExists()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            IceNumber = "ICE999999999"
        };
        
        _mockApplicationService
            .Setup(x => x.CreateClientAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("ICE number 'ICE999999999' is already in use"));

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
            CompanyName = "Test Company",
            IceNumber = "ICE456789",
            RcNumber = "RC456",
            VatNumber = "VAT456",
            CnssNumber = "CNSS456",
            Country = "Morocco",
            Address = "456 Business Ave",
            Industry = "Finance",
            AdminContactPerson = "Admin User",
            BillingContactPerson = "Billing User",
            Status = ClientStatus.Active,
            FiscalYearEnd = new DateTime(2024, 12, 31),
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
        result.CompanyName.Should().Be(client.CompanyName);
        result.IceNumber.Should().Be(client.IceNumber);
        result.RcNumber.Should().Be(client.RcNumber);
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
            CompanyName = "Updated Company Name",
            Country = "Morocco",
            Address = "456 New St, Rabat",
            IceNumber = "ICE789789789",
            RcNumber = "RC789",
            VatNumber = "VAT789",
            CnssNumber = "CNSS789",
            Industry = "Manufacturing",
            AdminContactPerson = "New Admin",
            BillingContactPerson = "New Billing",
            Status = "Inactive",
            FiscalYearEnd = "2025-06-30",
            AssignedTeamId = ""
        };
        
        var updatedClient = new Client
        {
            Id = clientId,
            TenantId = "tenant-123",
            CompanyName = request.CompanyName,
            Country = request.Country,
            Address = request.Address,
            IceNumber = request.IceNumber,
            RcNumber = request.RcNumber,
            VatNumber = request.VatNumber,
            CnssNumber = request.CnssNumber,
            Industry = request.Industry,
            AdminContactPerson = request.AdminContactPerson,
            BillingContactPerson = request.BillingContactPerson,
            Status = ClientStatus.Inactive,
            FiscalYearEnd = DateTime.Parse(request.FiscalYearEnd),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockApplicationService
            .Setup(x => x.UpdateClientAsync(
                clientId,
                "tenant-123",
                request.CompanyName,
                request.Country,
                request.Address,
                request.IceNumber,
                request.RcNumber,
                request.VatNumber,
                request.CnssNumber,
                request.Industry,
                request.AdminContactPerson,
                request.BillingContactPerson,
                ClientStatus.Inactive,
                request.FiscalYearEnd,
                request.AssignedTeamId))
            .ReturnsAsync(updatedClient);

        // Act
        var result = await _service.UpdateClient(request, CreateTestContext("tenant-123"));

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(clientId.ToString());
        result.CompanyName.Should().Be(request.CompanyName);
        result.IceNumber.Should().Be(request.IceNumber);
        result.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task DeleteClient_ShouldReturnSuccess_WhenClientDeleted()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new DeleteClientRequest { ClientId = clientId.ToString() };
        var deletedBy = "test-user";
        
        _mockUserContext
            .Setup(x => x.GetCurrentUserName())
            .Returns(deletedBy);
        
        _mockApplicationService
            .Setup(x => x.DeleteClientAsync(clientId, "tenant-123", It.IsAny<string>()))
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
            .Setup(x => x.GetCurrentUserName())
            .Returns("test-user");
        
        _mockApplicationService
            .Setup(x => x.DeleteClientAsync(clientId, "tenant-123", It.IsAny<string>()))
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
            new Client 
            { 
                Id = Guid.NewGuid(), 
                CompanyName = "Company A", 
                IceNumber = "ICE111111111",
                RcNumber = "RC111",
                Status = ClientStatus.Active 
            },
            new Client 
            { 
                Id = Guid.NewGuid(), 
                CompanyName = "Company B", 
                IceNumber = "ICE222222222",
                RcNumber = "RC222",
                Status = ClientStatus.Inactive 
            }
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
        result.Clients[0].CompanyName.Should().Be("Company A");
        result.Clients[1].CompanyName.Should().Be("Company B");
        result.Clients[0].IceNumber.Should().Be("ICE111111111");
        result.Clients[1].IceNumber.Should().Be("ICE222222222");
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

    [Fact]
    public async Task CreateClient_ShouldHandleEmptyOptionalFields()
    {
        // Arrange
        var request = new CreateClientRequest
        {
            TenantId = "tenant-123",
            CompanyName = "Minimal Company",
            // All optional fields are empty
            Country = "",
            Address = "",
            IceNumber = "",
            RcNumber = "",
            VatNumber = "",
            CnssNumber = "",
            Industry = "",
            AdminContactPerson = "",
            BillingContactPerson = "",
            FiscalYearEnd = "",
            AssignedTeamId = ""
        };
        
        var createdClient = new Client
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            CompanyName = request.CompanyName,
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _mockApplicationService
            .Setup(x => x.CreateClientAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(createdClient);

        // Act
        var result = await _service.CreateClient(request, CreateTestContext());

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(createdClient.Id.ToString());
        result.CompanyName.Should().Be(createdClient.CompanyName);
    }

    [Fact]
    public async Task UpdateClient_ShouldThrowNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new UpdateClientRequest
        {
            ClientId = clientId.ToString(),
            CompanyName = "Updated Company",
            Status = "Active"
        };
        
        _mockApplicationService
            .Setup(x => x.UpdateClientAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ClientStatus>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync((Client?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _service.UpdateClient(request, CreateTestContext("tenant-123")));
        
        exception.Status.StatusCode.Should().Be(StatusCode.NotFound);
    }
}