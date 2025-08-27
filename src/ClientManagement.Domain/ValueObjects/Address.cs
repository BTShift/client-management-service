namespace ClientManagement.Domain.ValueObjects;

public class Address : ValueObject
{
    public string StreetLine1 { get; }
    public string? StreetLine2 { get; }
    public string City { get; }
    public string? StateProvince { get; }
    public string? PostalCode { get; }
    public string Country { get; }

    private Address(
        string streetLine1,
        string? streetLine2,
        string city,
        string? stateProvince,
        string? postalCode,
        string country)
    {
        StreetLine1 = streetLine1;
        StreetLine2 = streetLine2;
        City = city;
        StateProvince = stateProvince;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(
        string streetLine1,
        string? streetLine2,
        string city,
        string? stateProvince,
        string? postalCode,
        string country)
    {
        if (string.IsNullOrWhiteSpace(streetLine1))
            throw new ArgumentException("Street line 1 cannot be empty", nameof(streetLine1));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(
            streetLine1.Trim(),
            streetLine2?.Trim(),
            city.Trim(),
            stateProvince?.Trim(),
            postalCode?.Trim(),
            country.Trim()
        );
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StreetLine1;
        yield return StreetLine2 ?? string.Empty;
        yield return City;
        yield return StateProvince ?? string.Empty;
        yield return PostalCode ?? string.Empty;
        yield return Country;
    }

    public override string ToString()
    {
        var parts = new List<string> { StreetLine1 };
        
        if (!string.IsNullOrWhiteSpace(StreetLine2))
            parts.Add(StreetLine2);
        
        parts.Add($"{City}{(StateProvince != null ? $", {StateProvince}" : "")} {PostalCode}".Trim());
        parts.Add(Country);
        
        return string.Join(", ", parts);
    }
}