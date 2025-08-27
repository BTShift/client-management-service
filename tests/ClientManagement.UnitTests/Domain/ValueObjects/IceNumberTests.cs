using ClientManagement.Domain.ValueObjects;

namespace ClientManagement.UnitTests.Domain.ValueObjects;

public class IceNumberTests
{
    [Fact]
    public void Create_WithValidIceNumber_ShouldCreateInstance()
    {
        // Arrange
        var validIce = "123456789012345";

        // Act
        var ice = IceNumber.Create(validIce);

        // Assert
        Assert.NotNull(ice);
        Assert.Equal(validIce, ice.Value);
    }

    [Theory]
    [InlineData("12345678901234")]  // Too short (14 digits)
    [InlineData("1234567890123456")] // Too long (16 digits)
    [InlineData("12345678901234A")]  // Contains letter
    [InlineData("123456789012 45")]  // Contains space
    public void Create_WithInvalidFormat_ShouldThrowException(string invalidIce)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => IceNumber.Create(invalidIce));
        Assert.Contains("Invalid ICE number format", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_WithEmptyValue_ShouldThrowException(string? emptyValue)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => IceNumber.Create(emptyValue!));
        Assert.Contains("ICE number cannot be empty", ex.Message);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var ice1 = IceNumber.Create("123456789012345");
        var ice2 = IceNumber.Create("123456789012345");

        // Act & Assert
        Assert.Equal(ice1, ice2);
        Assert.True(ice1 == ice2);
        Assert.False(ice1 != ice2);
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var ice1 = IceNumber.Create("123456789012345");
        var ice2 = IceNumber.Create("987654321098765");

        // Act & Assert
        Assert.NotEqual(ice1, ice2);
        Assert.False(ice1 == ice2);
        Assert.True(ice1 != ice2);
    }

    [Fact]
    public void ImplicitConversion_ToStringShould_ReturnValue()
    {
        // Arrange
        var ice = IceNumber.Create("123456789012345");

        // Act
        string value = ice;

        // Assert
        Assert.Equal("123456789012345", value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var ice = IceNumber.Create("123456789012345");

        // Act
        var result = ice.ToString();

        // Assert
        Assert.Equal("123456789012345", result);
    }
}