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
            CompanyName = "Test Company",
            Country = "Morocco",
            Address = "123 Test St, Casablanca",
            IceNumber = "ICE123456789",
            RcNumber = "RC12345",
            VatNumber = "VAT12345",
            CnssNumber = "CNSS12345",
            Industry = "Technology",
            AdminContactPerson = "John Doe",
            BillingContactPerson = "Jane Doe",
            Status = ClientStatus.Active,
            FiscalYearEnd = new DateTime(2024, 12, 31),
            AssignedTeamId = Guid.NewGuid()
        };

        // Act
        var result = await _repository.CreateAsync(client);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnClient_WhenExists()
    {
        // Arrange
        var client = new Client
        {
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            IceNumber = "ICE123456789",
            Status = ClientStatus.Active
        };
        
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(client.Id, "tenant-123");

        // Assert
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), "tenant-123");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateClient()
    {
        // Arrange
        var client = new Client
        {
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            IceNumber = "ICE123456789",
            Status = ClientStatus.Active
        };
        
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var updatedClient = new Client
        {
            Id = client.Id,
            TenantId = client.TenantId,
            CompanyName = "Updated Company",
            IceNumber = "ICE987654321",
            RcNumber = "RC54321",
            Status = ClientStatus.Inactive
        };

        var result = await _repository.UpdateAsync(updatedClient);

        // Assert
        result.Should().NotBeNull();
        result!.CompanyName.Should().Be("Updated Company");
        result.IceNumber.Should().Be("ICE987654321");
        result.Status.Should().Be(ClientStatus.Inactive);
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenClientNotExists()
    {
        // Arrange
        var client = new Client
        {
            Id = Guid.NewGuid(),
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            Status = ClientStatus.Active
        };

        // Act
        var result = await _repository.UpdateAsync(client);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteClient()
    {
        // Arrange
        var client = new Client
        {
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            IceNumber = "ICE123456789",
            Status = ClientStatus.Active
        };
        
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(client.Id, "tenant-123", "test-user");

        // Assert
        result.Should().BeTrue();
        
        // Query without filter to check soft delete
        var deletedClient = await _context.Clients
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == client.Id);
        
        deletedClient.Should().NotBeNull();
        deletedClient!.IsDeleted.Should().BeTrue();
        deletedClient.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        deletedClient.DeletedBy.Should().Be("test-user");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenClientNotExists()
    {
        // Act
        var result = await _repository.DeleteAsync(Guid.NewGuid(), "tenant-123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var clients = new[]
        {
            new Client { TenantId = "tenant-123", CompanyName = "Company A", IceNumber = "ICE111", Status = ClientStatus.Active },
            new Client { TenantId = "tenant-123", CompanyName = "Company B", IceNumber = "ICE222", Status = ClientStatus.Active },
            new Client { TenantId = "tenant-123", CompanyName = "Company C", IceNumber = "ICE333", Status = ClientStatus.Active },
            new Client { TenantId = "other-tenant", CompanyName = "Company D", IceNumber = "ICE444", Status = ClientStatus.Active }
        };
        
        await _context.Clients.AddRangeAsync(clients);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync("tenant-123", page: 1, pageSize: 2);

        // Assert
        result.Items.Count.Should().Be(2);
        result.TotalCount.Should().Be(3);
        result.Items[0].CompanyName.Should().Be("Company A");
        result.Items[1].CompanyName.Should().Be("Company B");
    }

    [Fact]
    public async Task ListAsync_ShouldFilterBySoftDelete()
    {
        // Arrange
        var clients = new[]
        {
            new Client { TenantId = "tenant-123", CompanyName = "Company A", Status = ClientStatus.Active },
            new Client { TenantId = "tenant-123", CompanyName = "Company B", Status = ClientStatus.Active, IsDeleted = true },
        };
        
        await _context.Clients.AddRangeAsync(clients);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync("tenant-123");

        // Assert
        result.Items.Count.Should().Be(1);
        result.TotalCount.Should().Be(1);
        result.Items[0].CompanyName.Should().Be("Company A");
    }

    [Fact]
    public async Task ListAsync_ShouldFilterBySearchTerm()
    {
        // Arrange
        var clients = new[]
        {
            new Client { TenantId = "tenant-123", CompanyName = "ABC Company", IceNumber = "ICE123", Status = ClientStatus.Active },
            new Client { TenantId = "tenant-123", CompanyName = "XYZ Company", IceNumber = "ICE456", Status = ClientStatus.Active },
            new Client { TenantId = "tenant-123", CompanyName = "Test Corp", RcNumber = "ABC789", Status = ClientStatus.Active }
        };
        
        await _context.Clients.AddRangeAsync(clients);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync("tenant-123", searchTerm: "ABC");

        // Assert
        result.Items.Count.Should().Be(2);
        result.Items.Should().Contain(c => c.CompanyName == "ABC Company");
        result.Items.Should().Contain(c => c.RcNumber == "ABC789");
    }

    [Fact]
    public async Task IceNumberExistsAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var client = new Client
        {
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            IceNumber = "ICE123456789",
            Status = ClientStatus.Active
        };
        
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.IceNumberExistsAsync("ICE123456789", "tenant-123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IceNumberExistsAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _repository.IceNumberExistsAsync("ICE999999999", "tenant-123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IceNumberExistsAsync_ShouldExcludeSpecifiedClient()
    {
        // Arrange
        var client = new Client
        {
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            IceNumber = "ICE123456789",
            Status = ClientStatus.Active
        };
        
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.IceNumberExistsAsync("ICE123456789", "tenant-123", client.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RcNumberExistsAsync_ShouldReturnTrue_WhenExists()
    {
        // Arrange
        var client = new Client
        {
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            RcNumber = "RC12345",
            Status = ClientStatus.Active
        };
        
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.RcNumberExistsAsync("RC12345", "tenant-123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RcNumberExistsAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Act
        var result = await _repository.RcNumberExistsAsync("RC99999", "tenant-123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RcNumberExistsAsync_ShouldExcludeSpecifiedClient()
    {
        // Arrange
        var client = new Client
        {
            TenantId = "tenant-123",
            CompanyName = "Test Company",
            RcNumber = "RC12345",
            Status = ClientStatus.Active
        };
        
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.RcNumberExistsAsync("RC12345", "tenant-123", client.Id);

        // Assert
        result.Should().BeFalse();
    }
}