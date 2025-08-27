using ClientManagement.Domain.ValueObjects;

namespace ClientManagement.UnitTests.Domain.ValueObjects;

public class ContactInfoTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateInstance()
    {
        // Arrange
        var name = "John Doe";
        var email = "john@example.com";
        var phone = "+212600000000";
        var title = "Manager";

        // Act
        var contact = ContactInfo.Create(name, email, phone, title);

        // Assert
        Assert.NotNull(contact);
        Assert.Equal(name, contact.Name);
        Assert.Equal(email.ToLowerInvariant(), contact.Email);
        Assert.Equal(phone, contact.Phone);
        Assert.Equal(title, contact.Title);
    }

    [Fact]
    public void Create_WithMinimalData_ShouldCreateInstance()
    {
        // Arrange
        var name = "Jane Doe";
        var email = "jane@example.com";

        // Act
        var contact = ContactInfo.Create(name, email);

        // Assert
        Assert.NotNull(contact);
        Assert.Equal(name, contact.Name);
        Assert.Equal(email.ToLowerInvariant(), contact.Email);
        Assert.Null(contact.Phone);
        Assert.Null(contact.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithInvalidName_ShouldThrowException(string? invalidName)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            ContactInfo.Create(invalidName!, "test@example.com"));
        Assert.Contains("Contact name cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithInvalidEmail_ShouldThrowException(string? invalidEmail)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            ContactInfo.Create("John Doe", invalidEmail!));
        Assert.Contains("Contact email cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user @example.com")]
    public void Create_WithInvalidEmailFormat_ShouldThrowException(string invalidEmail)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            ContactInfo.Create("John Doe", invalidEmail));
        Assert.Contains("Invalid email format", ex.Message);
    }

    [Theory]
    [InlineData("1234567")]  // Too short
    [InlineData("1234567890123456")] // Too long
    [InlineData("123-45a-6789")]  // Contains letters after normalization
    public void Create_WithInvalidPhoneFormat_ShouldThrowException(string invalidPhone)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() =>
            ContactInfo.Create("John Doe", "john@example.com", invalidPhone));
        Assert.Contains("Invalid phone format", ex.Message);
    }

    [Theory]
    [InlineData("+212 600 000 000")]
    [InlineData("+212-600-000-000")]
    [InlineData("(212) 600000000")]
    [InlineData("212600000000")]
    public void Create_WithValidPhoneFormats_ShouldNormalizeAndCreate(string validPhone)
    {
        // Act
        var contact = ContactInfo.Create("John Doe", "john@example.com", validPhone);

        // Assert
        Assert.NotNull(contact);
        Assert.NotNull(contact.Phone);
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var contact1 = ContactInfo.Create("John Doe", "JOHN@EXAMPLE.COM", "+212600000000", "Manager");
        var contact2 = ContactInfo.Create("John Doe", "john@example.com", "+212600000000", "Manager");

        // Act & Assert
        Assert.Equal(contact1, contact2);
        Assert.True(contact1 == contact2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var contact1 = ContactInfo.Create("John Doe", "john@example.com");
        var contact2 = ContactInfo.Create("Jane Doe", "jane@example.com");

        // Act & Assert
        Assert.NotEqual(contact1, contact2);
        Assert.True(contact1 != contact2);
    }

    [Fact]
    public void ToString_WithAllFields_ShouldFormatCorrectly()
    {
        // Arrange
        var contact = ContactInfo.Create("John Doe", "john@example.com", "+212600000000", "Manager");

        // Act
        var result = contact.ToString();

        // Assert
        Assert.Contains("Manager", result);
        Assert.Contains("John Doe", result);
        Assert.Contains("john@example.com", result);
        Assert.Contains("+212600000000", result);
    }

    [Fact]
    public void ToString_WithMinimalFields_ShouldFormatCorrectly()
    {
        // Arrange
        var contact = ContactInfo.Create("Jane Doe", "jane@example.com");

        // Act
        var result = contact.ToString();

        // Assert
        Assert.Contains("Jane Doe", result);
        Assert.Contains("jane@example.com", result);
    }
}