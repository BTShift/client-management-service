using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ClientManagement.Application.Services;
using ClientManagement.Application.Interfaces;
using ClientManagement.Domain.Entities;

namespace ClientManagement.UnitTests.Application;

public class ClientGroupApplicationServiceTests
{
    private readonly Mock<IClientGroupRepository> _mockRepository;
    private readonly Mock<ILogger<ClientGroupApplicationService>> _mockLogger;
    private readonly ClientGroupApplicationService _service;

    public ClientGroupApplicationServiceTests()
    {
        _mockRepository = new Mock<IClientGroupRepository>();
        _mockLogger = new Mock<ILogger<ClientGroupApplicationService>>();
        _service = new ClientGroupApplicationService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateGroupAsync_WithValidData_ShouldCreateGroup()
    {
        // Arrange
        var tenantId = "test-tenant";
        var groupName = "Sales Team";
        var description = "Group for sales department";
        
        _mockRepository
            .Setup(r => r.GroupNameExistsAsync(groupName, tenantId, null))
            .ReturnsAsync(false);
        
        _mockRepository
            .Setup(r => r.CreateGroupAsync(It.IsAny<ClientGroup>()))
            .ReturnsAsync((ClientGroup g) => g);

        // Act
        var result = await _service.CreateGroupAsync(tenantId, groupName, description);

        // Assert
        result.Should().NotBeNull();
        result.TenantId.Should().Be(tenantId);
        result.Name.Should().Be(groupName);
        result.Description.Should().Be(description);
        
        _mockRepository.Verify(r => r.CreateGroupAsync(It.IsAny<ClientGroup>()), Times.Once);
    }

    [Fact]
    public async Task CreateGroupAsync_WithDuplicateName_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tenantId = "test-tenant";
        var groupName = "Sales Team";
        var description = "Group for sales department";
        
        _mockRepository
            .Setup(r => r.GroupNameExistsAsync(groupName, tenantId, null))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateGroupAsync(tenantId, groupName, description)
        );
        
        _mockRepository.Verify(r => r.CreateGroupAsync(It.IsAny<ClientGroup>()), Times.Never);
    }

    [Fact]
    public async Task AddClientToGroupAsync_WithValidIds_ShouldReturnTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var tenantId = "test-tenant";
        
        _mockRepository
            .Setup(r => r.AddClientToGroupAsync(clientId, groupId, tenantId, null))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AddClientToGroupAsync(clientId, groupId, tenantId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.AddClientToGroupAsync(clientId, groupId, tenantId, null), Times.Once);
    }

    [Fact]
    public async Task RemoveClientFromGroupAsync_WithValidIds_ShouldReturnTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var tenantId = "test-tenant";
        
        _mockRepository
            .Setup(r => r.RemoveClientFromGroupAsync(clientId, groupId, tenantId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RemoveClientFromGroupAsync(clientId, groupId, tenantId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.RemoveClientFromGroupAsync(clientId, groupId, tenantId), Times.Once);
    }

    [Fact]
    public async Task ListGroupsAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var tenantId = "test-tenant";
        var groups = new List<ClientGroup>
        {
            new ClientGroup { Id = Guid.NewGuid(), Name = "Group 1", TenantId = tenantId },
            new ClientGroup { Id = Guid.NewGuid(), Name = "Group 2", TenantId = tenantId }
        };
        
        _mockRepository
            .Setup(r => r.ListGroupsAsync(tenantId, 1, 20, null))
            .ReturnsAsync((groups, groups.Count));

        // Act
        var (resultGroups, totalCount) = await _service.ListGroupsAsync(tenantId);

        // Assert
        resultGroups.Should().HaveCount(2);
        totalCount.Should().Be(2);
        _mockRepository.Verify(r => r.ListGroupsAsync(tenantId, 1, 20, null), Times.Once);
    }
}