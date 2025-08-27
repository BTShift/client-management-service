using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ClientManagement.Infrastructure.Data;
using ClientManagement.Infrastructure.Repositories;
using ClientManagement.Domain.Entities;

namespace ClientManagement.IntegrationTests.Infrastructure;

public class ClientRepositoryTests : IDisposable
{
    private readonly ClientManagementDbContext _context;
    private readonly ClientRepository _repository;

    public ClientRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ClientManagementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ClientManagementDbContext(options);
        _repository = new ClientRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateClient()
    {
        // Arrange
        var client = new Client
        {
            TenantId = "tenant-123",
            Name = "Test Client",
            Email = "test@example.com",
            Phone = "123-456-7890",
            Address = "123 Test St",
            Status = ClientStatus.Active
        };

        // Act
        var result = await _repository.CreateAsync(client);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        // Verify in database
        var dbClient = await _context.Clients.FirstOrDefaultAsync(c => c.Id == result.Id);
        dbClient.Should().NotBeNull();
        dbClient!.Name.Should().Be("Test Client");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnClient_WhenExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            TenantId = "tenant-123",
            Name = "Test Client",
            Email = "test@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(clientId, "tenant-123");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(clientId);
        result.Name.Should().Be("Test Client");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenWrongTenant()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            TenantId = "tenant-123",
            Name = "Test Client",
            Email = "test@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(clientId, "different-tenant");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var originalClient = new Client
        {
            Id = clientId,
            TenantId = "tenant-123",
            Name = "Original Name",
            Email = "original@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        
        _context.Clients.Add(originalClient);
        await _context.SaveChangesAsync();
        
        // Detach the entity to simulate a disconnected scenario
        _context.Entry(originalClient).State = EntityState.Detached;
        
        var updateClient = new Client
        {
            Id = clientId,
            TenantId = "tenant-123",
            Name = "Updated Name",
            Email = "updated@example.com",
            Phone = "999-999-9999",
            Status = ClientStatus.Inactive
        };

        // Act
        var result = await _repository.UpdateAsync(updateClient);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
        result.Email.Should().Be("updated@example.com");
        result.Phone.Should().Be("999-999-9999");
        result.Status.Should().Be(ClientStatus.Inactive);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        // Verify in database
        var dbClient = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clientId);
        dbClient!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            TenantId = "tenant-123",
            Name = "Test Client",
            Email = "test@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(clientId, "tenant-123");

        // Assert
        result.Should().BeTrue();
        
        // Verify soft delete in database
        var dbClient = await _context.Clients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == clientId);
        
        dbClient.Should().NotBeNull();
        dbClient!.IsDeleted.Should().BeTrue();
        dbClient.DeletedAt.Should().NotBeNull();
        dbClient.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ListAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var tenantId = "tenant-123";
        for (int i = 1; i <= 25; i++)
        {
            _context.Clients.Add(new Client
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"Client {i:D2}",
                Email = $"client{i}@example.com",
                Status = ClientStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        
        // Add clients from different tenant (should not be included)
        _context.Clients.Add(new Client
        {
            Id = Guid.NewGuid(),
            TenantId = "different-tenant",
            Name = "Other Client",
            Email = "other@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.ListAsync(tenantId, 2, 10);

        // Assert
        items.Should().HaveCount(10);
        totalCount.Should().Be(25);
        items[0].Name.Should().Be("Client 11"); // Page 2, ordered by name
    }

    [Fact]
    public async Task ListAsync_ShouldFilterBySearchTerm()
    {
        // Arrange
        var tenantId = "tenant-123";
        _context.Clients.AddRange(
            new Client { Id = Guid.NewGuid(), TenantId = tenantId, Name = "John Doe", Email = "john@example.com", Status = ClientStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Client { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Jane Smith", Email = "jane@example.com", Status = ClientStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Client { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Bob Johnson", Email = "bob@test.com", Status = ClientStatus.Active, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.ListAsync(tenantId, 1, 10, "example.com");

        // Assert
        items.Should().HaveCount(2); // Both John and Jane have example.com in email
        totalCount.Should().Be(2);
        items.Any(c => c.Name == "John Doe").Should().BeTrue();
        items.Any(c => c.Name == "Jane Smith").Should().BeTrue();
    }

    [Fact]
    public async Task ListAsync_ShouldExcludeSoftDeletedClients()
    {
        // Arrange
        var tenantId = "tenant-123";
        _context.Clients.AddRange(
            new Client { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Active Client", Email = "active@example.com", Status = ClientStatus.Active, IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Client { Id = Guid.NewGuid(), TenantId = tenantId, Name = "Deleted Client", Email = "deleted@example.com", Status = ClientStatus.Active, IsDeleted = true, DeletedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.ListAsync(tenantId, 1, 10);

        // Assert
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        items[0].Name.Should().Be("Active Client");
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        var tenantId = "tenant-123";
        var client = new Client
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = "Test Client",
            Email = "existing@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync("existing@example.com", tenantId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        // Arrange & Act
        var result = await _repository.EmailExistsAsync("nonexistent@example.com", "tenant-123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldExcludeSpecifiedClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tenantId = "tenant-123";
        var client = new Client
        {
            Id = clientId,
            TenantId = tenantId,
            Name = "Test Client",
            Email = "test@example.com",
            Status = ClientStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync("test@example.com", tenantId, clientId);

        // Assert
        result.Should().BeFalse(); // Should exclude the client with the specified ID
    }
}