using ClientManagement.Domain.Entities;
using ClientManagement.Domain.ValueObjects;

namespace ClientManagement.UnitTests.Domain.Entities;

public class ClientTests
{
    private readonly ContactInfo _validAdminContact;
    private readonly Address _validAddress;

    public ClientTests()
    {
        _validAdminContact = ContactInfo.Create("John Doe", "john@example.com", "+212600000000", "Admin");
        _validAddress = Address.Create("123 Main St", null, "Casablanca", null, "20000", "Morocco");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateClient()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var tenantId = Guid.NewGuid().ToString();
        var companyName = "Test Company";
        var country = "Morocco";

        // Act
        var client = Client.Create(
            id,
            tenantId,
            companyName,
            country,
            _validAddress,
            Industry.Technology,
            _validAdminContact);

        // Assert
        Assert.NotNull(client);
        Assert.Equal(id, client.Id);
        Assert.Equal(tenantId, client.TenantId);
        Assert.Equal(companyName, client.CompanyName);
        Assert.Equal(country, client.Country);
        Assert.Equal(Industry.Technology, client.Industry);
        Assert.Equal(ClientStatus.Prospect, client.Status);
        Assert.Equal(12, client.FiscalYearEndMonth);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithInvalidId_ShouldThrowException(string? invalidId)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Client.Create(
                invalidId!,
                "tenant-1",
                "Company",
                "Morocco",
                _validAddress,
                Industry.Technology,
                _validAdminContact));
        
        Assert.Contains("Client ID cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithInvalidCompanyName_ShouldThrowException(string? invalidName)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Client.Create(
                "id-1",
                "tenant-1",
                invalidName!,
                "Morocco",
                _validAddress,
                Industry.Technology,
                _validAdminContact));
        
        Assert.Contains("Company name cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Create_WithInvalidFiscalYearEndMonth_ShouldThrowException(int invalidMonth)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            Client.Create(
                "id-1",
                "tenant-1",
                "Company",
                "Morocco",
                _validAddress,
                Industry.Technology,
                _validAdminContact,
                ClientStatus.Active,
                invalidMonth));
        
        Assert.Contains("Fiscal year end month must be between 1 and 12", ex.Message);
    }

    [Fact]
    public void SetMoroccanIdentifiers_ShouldUpdateIdentifiers()
    {
        // Arrange
        var client = CreateValidClient();
        var ice = IceNumber.Create("123456789012345");
        var rc = RcNumber.Create("RC12345");
        var vat = VatNumber.Create("12345678");
        var cnss = CnssNumber.Create("123456789");

        // Act
        client.SetMoroccanIdentifiers(ice, rc, vat, cnss);

        // Assert
        Assert.Equal(ice, client.IceNumber);
        Assert.Equal(rc, client.RcNumber);
        Assert.Equal(vat, client.VatNumber);
        Assert.Equal(cnss, client.CnssNumber);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldChangeStatusToActive()
    {
        // Arrange
        var client = CreateValidClient();
        client.UpdateStatus(ClientStatus.Inactive);

        // Act
        client.Activate();

        // Assert
        Assert.Equal(ClientStatus.Active, client.Status);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowException()
    {
        // Arrange
        var client = CreateValidClient();
        client.UpdateStatus(ClientStatus.Active);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => client.Activate());
        Assert.Contains("Client is already active", ex.Message);
    }

    [Fact]
    public void AssignUser_ShouldAddUserToCollection()
    {
        // Arrange
        var client = CreateValidClient();
        var userId = "user-123";

        // Act
        client.AssignUser(userId);

        // Assert
        Assert.Contains(userId, client.UserIds);
        Assert.Single(client.UserIds);
    }

    [Fact]
    public void AssignUser_Duplicate_ShouldNotAddTwice()
    {
        // Arrange
        var client = CreateValidClient();
        var userId = "user-123";

        // Act
        client.AssignUser(userId);
        client.AssignUser(userId);

        // Assert
        Assert.Contains(userId, client.UserIds);
        Assert.Single(client.UserIds);
    }

    [Fact]
    public void RemoveUser_ShouldRemoveUserFromCollection()
    {
        // Arrange
        var client = CreateValidClient();
        var userId = "user-123";
        client.AssignUser(userId);

        // Act
        client.RemoveUser(userId);

        // Assert
        Assert.DoesNotContain(userId, client.UserIds);
        Assert.Empty(client.UserIds);
    }

    [Fact]
    public void AddToGroup_ShouldAddGroupToCollection()
    {
        // Arrange
        var client = CreateValidClient();
        var groupId = "group-123";

        // Act
        client.AddToGroup(groupId);

        // Assert
        Assert.Contains(groupId, client.GroupIds);
        Assert.Single(client.GroupIds);
    }

    [Fact]
    public void UpdateAddress_ShouldUpdateClientAddress()
    {
        // Arrange
        var client = CreateValidClient();
        var newAddress = Address.Create("456 New St", "Suite 100", "Rabat", null, "10000", "Morocco");

        // Act
        client.UpdateAddress(newAddress);

        // Assert
        Assert.Equal(newAddress, client.Address);
    }

    [Fact]
    public void UpdateAddress_WithNull_ShouldThrowException()
    {
        // Arrange
        var client = CreateValidClient();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => client.UpdateAddress(null!));
    }

    private Client CreateValidClient()
    {
        return Client.Create(
            Guid.NewGuid().ToString(),
            "tenant-1",
            "Test Company",
            "Morocco",
            _validAddress,
            Industry.Technology,
            _validAdminContact,
            ClientStatus.Prospect,
            12);
    }
}