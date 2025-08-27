using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ClientManagement.Application.Services;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;

namespace ClientManagement.UnitTests.Application;

public class ClientApplicationServiceTests
{
    private readonly Mock<IClientRepository> _mockRepository;
    private readonly Mock<ILogger<ClientApplicationService>> _mockLogger;
    private readonly ClientApplicationService _service;

    public ClientApplicationServiceTests()
    {
        _mockRepository = new Mock<IClientRepository>();
        _mockLogger = new Mock<ILogger<ClientApplicationService>>();
        _service = new ClientApplicationService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldCreateClient_WhenEmailIsUnique()
    {
        // Arrange
        var tenantId = "tenant-123";
        var name = "John Doe";
        var email = "john@example.com";
        var phone = "123-456-7890";
        var address = "123 Main St";
        
        _mockRepository
            .Setup(x => x.EmailExistsAsync(email, tenantId, null))
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
        var result = await _service.CreateClientAsync(tenantId, name, email, phone, address);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Email.Should().Be(email);
        result.Phone.Should().Be(phone);
        result.Address.Should().Be(address);
        result.TenantId.Should().Be(tenantId);
        result.Status.Should().Be(ClientStatus.Active);
        
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<Client>()), Times.Once);
    }

    [Fact]
    public async Task CreateClientAsync_ShouldThrowException_WhenEmailExists()
    {
        // Arrange
        var tenantId = "tenant-123";
        var email = "existing@example.com";
        
        _mockRepository
            .Setup(x => x.EmailExistsAsync(email, tenantId, null))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateClientAsync(tenantId, "Name", email, "", ""));
        
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
            Name = "John Doe",
            Email = "john@example.com"
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
    public async Task UpdateClientAsync_ShouldUpdateClient_WhenClientExistsAndEmailIsUnique()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var updatedClient = new Client
        {
            Id = clientId,
            TenantId = tenantId,
            Name = "Updated Name",
            Email = "updated@example.com",
            Status = ClientStatus.Inactive
        };
        
        _mockRepository
            .Setup(x => x.EmailExistsAsync(updatedClient.Email, tenantId, clientId))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Client>()))
            .ReturnsAsync(updatedClient);

        // Act
        var result = await _service.UpdateClientAsync(
            clientId, tenantId, updatedClient.Name, updatedClient.Email,
            "", "", ClientStatus.Inactive);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updatedClient.Name);
        result.Email.Should().Be(updatedClient.Email);
        result.Status.Should().Be(ClientStatus.Inactive);
        
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Client>()), Times.Once);
    }

    [Fact]
    public async Task UpdateClientAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var email = "existing@example.com";
        
        _mockRepository
            .Setup(x => x.EmailExistsAsync(email, tenantId, clientId))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateClientAsync(clientId, tenantId, "Name", email, "", "", ClientStatus.Active));
        
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task DeleteClientAsync_ShouldReturnTrue_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        
        _mockRepository
            .Setup(x => x.DeleteAsync(clientId, tenantId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteClientAsync(clientId, tenantId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(clientId, tenantId), Times.Once);
    }

    [Fact]
    public async Task DeleteClientAsync_ShouldReturnFalse_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        
        _mockRepository
            .Setup(x => x.DeleteAsync(clientId, tenantId))
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
            new Client { Id = Guid.NewGuid(), Name = "Client 1", TenantId = tenantId },
            new Client { Id = Guid.NewGuid(), Name = "Client 2", TenantId = tenantId }
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
}