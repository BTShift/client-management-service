using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ClientManagement.Application.Services;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;
using MassTransit;

namespace ClientManagement.UnitTests.Application;

public class ClientApplicationServiceTests
{
    private readonly Mock<IClientRepository> _mockRepository;
    private readonly Mock<ILogger<ClientApplicationService>> _mockLogger;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly ClientApplicationService _service;

    public ClientApplicationServiceTests()
    {
        _mockRepository = new Mock<IClientRepository>();
        _mockLogger = new Mock<ILogger<ClientApplicationService>>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockUserContext = new Mock<IUserContext>();
        
        // Setup default user context behavior
        _mockUserContext.Setup(x => x.IsAuthenticated()).Returns(true);
        _mockUserContext.Setup(x => x.GetCurrentUserName()).Returns("test-user");
        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns("user-123");
        _mockUserContext.Setup(x => x.GetCurrentTenantId()).Returns("tenant-123");
        
        _service = new ClientApplicationService(
            _mockRepository.Object, 
            _mockLogger.Object, 
            _mockPublishEndpoint.Object,
            _mockUserContext.Object);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldCreateClient_WhenIceAndRcAreUnique()
    {
        // Arrange
        var tenantId = "tenant-123";
        var companyName = "Test Company Ltd";
        var country = "Morocco";
        var address = "123 Main St, Casablanca";
        var iceNumber = "ICE123456789";
        var rcNumber = "RC12345";
        var vatNumber = "VAT12345";
        var cnssNumber = "CNSS12345";
        var industry = "Technology";
        var adminContact = "John Doe";
        var billingContact = "Jane Doe";
        var fiscalYearEnd = "2024-12-31";
        var assignedTeamId = Guid.NewGuid().ToString();
        
        _mockRepository
            .Setup(x => x.IceNumberExistsAsync(iceNumber, tenantId, null))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.RcNumberExistsAsync(rcNumber, tenantId, null))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Client>()))
            .ReturnsAsync((Client c) =>
            {
                c.Id = Guid.NewGuid();
                c.CreatedAt = DateTime.UtcNow;
                c.UpdatedAt = DateTime.UtcNow;
                return c;
            });

        // Act
        var result = await _service.CreateClientAsync(
            tenantId, companyName, country, address, 
            iceNumber, rcNumber, vatNumber, cnssNumber, 
            industry, adminContact, billingContact, 
            fiscalYearEnd, assignedTeamId);

        // Assert
        result.Should().NotBeNull();
        result.CompanyName.Should().Be(companyName);
        result.IceNumber.Should().Be(iceNumber);
        result.RcNumber.Should().Be(rcNumber);
        result.VatNumber.Should().Be(vatNumber);
        result.CnssNumber.Should().Be(cnssNumber);
        result.TenantId.Should().Be(tenantId);
        result.Status.Should().Be(ClientStatus.Active);
        
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Client>()), Times.Once);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldThrowException_WhenIceNumberExists()
    {
        // Arrange
        var tenantId = "tenant-123";
        var iceNumber = "ICE987654321";
        
        _mockRepository
            .Setup(x => x.IceNumberExistsAsync(iceNumber, tenantId, null))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateClientAsync(
                tenantId, "Company", "Morocco", "Address",
                iceNumber, "RC999", "VAT999", "CNSS999",
                "Industry", "Admin", "Billing",
                "2024-12-31", ""));
        
        exception.Message.Should().Contain("ICE number");
        exception.Message.Should().Contain("already in use");
        
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldThrowException_WhenRcNumberExists()
    {
        // Arrange
        var tenantId = "tenant-123";
        var iceNumber = "ICE111111111";
        var rcNumber = "RC99999";
        
        _mockRepository
            .Setup(x => x.IceNumberExistsAsync(iceNumber, tenantId, null))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.RcNumberExistsAsync(rcNumber, tenantId, null))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateClientAsync(
                tenantId, "Company", "Morocco", "Address",
                iceNumber, rcNumber, "VAT999", "CNSS999",
                "Industry", "Admin", "Billing",
                "2024-12-31", ""));
        
        exception.Message.Should().Contain("RC number");
        exception.Message.Should().Contain("already in use");
        
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task GetClientAsync_ShouldReturnClient_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var expectedClient = new Client
        {
            Id = clientId,
            TenantId = tenantId,
            CompanyName = "Test Company",
            IceNumber = "ICE123456789",
            Status = ClientStatus.Active
        };
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(clientId, tenantId))
            .ReturnsAsync(expectedClient);

        // Act
        var result = await _service.GetClientAsync(clientId, tenantId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedClient);
    }

    [Fact]
    public async Task GetClientAsync_ShouldReturnNull_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(clientId, tenantId))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _service.GetClientAsync(clientId, tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateClientAsync_ShouldUpdateClient_WhenClientExistsAndIceRcAreUnique()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var updatedClient = new Client
        {
            Id = clientId,
            TenantId = tenantId,
            CompanyName = "Updated Company",
            IceNumber = "ICE999999999",
            RcNumber = "RC99999",
            Status = ClientStatus.Inactive
        };
        
        _mockRepository
            .Setup(x => x.IceNumberExistsAsync(updatedClient.IceNumber, tenantId, clientId))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.RcNumberExistsAsync(updatedClient.RcNumber, tenantId, clientId))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Client>()))
            .ReturnsAsync(updatedClient);

        // Act
        var result = await _service.UpdateClientAsync(
            clientId, tenantId, updatedClient.CompanyName, "Morocco", "New Address",
            updatedClient.IceNumber, updatedClient.RcNumber, "VAT999", "CNSS999",
            "Technology", "Admin", "Billing",
            ClientStatus.Inactive, "2024-12-31", "");

        // Assert
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be(updatedClient.CompanyName);
        result.IceNumber.Should().Be(updatedClient.IceNumber);
        result.RcNumber.Should().Be(updatedClient.RcNumber);
        result.Status.Should().Be(ClientStatus.Inactive);
        
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Client>()), Times.Once);
    }

    [Fact]
    public async Task UpdateClientAsync_ShouldThrowException_WhenIceNumberAlreadyExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var iceNumber = "ICE555555555";
        
        _mockRepository
            .Setup(x => x.IceNumberExistsAsync(iceNumber, tenantId, clientId))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateClientAsync(
                clientId, tenantId, "Company", "Morocco", "Address",
                iceNumber, "RC123", "VAT123", "CNSS123",
                "Industry", "Admin", "Billing",
                ClientStatus.Active, "2024-12-31", ""));
        
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_ShouldThrowException_WhenRcNumberAlreadyExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var iceNumber = "ICE666666666";
        var rcNumber = "RC88888";
        
        _mockRepository
            .Setup(x => x.IceNumberExistsAsync(iceNumber, tenantId, clientId))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.RcNumberExistsAsync(rcNumber, tenantId, clientId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateClientAsync(
                clientId, tenantId, "Company", "Morocco", "Address",
                iceNumber, rcNumber, "VAT123", "CNSS123",
                "Industry", "Admin", "Billing",
                ClientStatus.Active, "2024-12-31", ""));
        
        exception.Message.Should().Contain("RC number");
        exception.Message.Should().Contain("already in use");
        
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task DeleteClientAsync_ShouldReturnTrue_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var deletedBy = "user@example.com";
        
        _mockRepository
            .Setup(x => x.DeleteAsync(clientId, tenantId, It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteClientAsync(clientId, tenantId, deletedBy);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(clientId, tenantId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteClientAsync_ShouldReturnFalse_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        
        _mockRepository
            .Setup(x => x.DeleteAsync(clientId, tenantId, It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteClientAsync(clientId, tenantId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ListClientsAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var tenantId = "tenant-123";
        var clients = new List<Client>
        {
            new Client { Id = Guid.NewGuid(), CompanyName = "Company A", TenantId = tenantId },
            new Client { Id = Guid.NewGuid(), CompanyName = "Company B", TenantId = tenantId }
        };
        
        _mockRepository
            .Setup(x => x.ListAsync(tenantId, 1, 20, null))
            .ReturnsAsync((clients, 2));

        // Act
        var (items, totalCount) = await _service.ListClientsAsync(tenantId, 1, 20);

        // Assert
        items.Should().HaveCount(2);
        totalCount.Should().Be(2);
        _mockRepository.Verify(x => x.ListAsync(tenantId, 1, 20, null), Times.Once);
    }

    [Theory]
    [InlineData(0, 20, 1, 20)]     // Invalid page -> defaults to 1
    [InlineData(1, 0, 1, 20)]      // Invalid page size -> defaults to 20
    [InlineData(1, 150, 1, 100)]   // Page size too large -> capped at 100
    public async Task ListClientsAsync_ShouldHandleInvalidPagination(
        int inputPage, int inputPageSize, int expectedPage, int expectedPageSize)
    {
        // Arrange
        var tenantId = "tenant-123";
        _mockRepository
            .Setup(x => x.ListAsync(tenantId, expectedPage, expectedPageSize, null))
            .ReturnsAsync((new List<Client>(), 0));

        // Act
        await _service.ListClientsAsync(tenantId, inputPage, inputPageSize);

        // Assert
        _mockRepository.Verify(x => x.ListAsync(tenantId, expectedPage, expectedPageSize, null), Times.Once);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldNotValidateIceNumber_WhenEmpty()
    {
        // Arrange
        var tenantId = "tenant-123";
        
        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<Client>()))
            .ReturnsAsync((Client c) =>
            {
                c.Id = Guid.NewGuid();
                c.CreatedAt = DateTime.UtcNow;
                c.UpdatedAt = DateTime.UtcNow;
                return c;
            });

        // Act
        var result = await _service.CreateClientAsync(
            tenantId, "Company", "Morocco", "Address",
            "", "", "", "",  // All business numbers empty
            "", "", "",
            "2024-12-31", "");

        // Assert
        result.Should().NotBeNull();
        
        // Should not check existence for empty values
        _mockRepository.Verify(x => x.IceNumberExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
        _mockRepository.Verify(x => x.RcNumberExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_ShouldReturnNull_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        
        _mockRepository
            .Setup(x => x.IceNumberExistsAsync(It.IsAny<string>(), tenantId, clientId))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.RcNumberExistsAsync(It.IsAny<string>(), tenantId, clientId))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Client>()))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _service.UpdateClientAsync(
            clientId, tenantId, "Company", "Morocco", "Address",
            "ICE777777777", "RC77777", "VAT777", "CNSS777",
            "Industry", "Admin", "Billing",
            ClientStatus.Active, "2024-12-31", "");

        // Assert
        result.Should().BeNull();
    }
}