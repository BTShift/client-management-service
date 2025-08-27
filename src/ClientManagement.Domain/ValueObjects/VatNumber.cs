namespace ClientManagement.Domain.ValueObjects;

public class VatNumber : ValueObject
{
    public string Value { get; }

    private VatNumber(string value)
    {
        Value = value;
    }

    public static VatNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("VAT number cannot be empty", nameof(value));

        value = value.Trim();

        if (!IsValidVatNumber(value))
            throw new ArgumentException($"Invalid VAT number format: {value}", nameof(value));

        return new VatNumber(value);
    }

    private static bool IsValidVatNumber(string value)
    {
        // Moroccan VAT number (Numéro de TVA) is typically an 8-digit number
        if (value.Length < 8 || value.Length > 15)
            return false;

        // Should be primarily numeric
        return value.Count(char.IsDigit) >= 8;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(VatNumber vat) => vat.Value;
}