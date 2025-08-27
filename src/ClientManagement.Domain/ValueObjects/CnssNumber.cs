namespace ClientManagement.Domain.ValueObjects;

public class CnssNumber : ValueObject
{
    public string Value { get; }

    private CnssNumber(string value)
    {
        Value = value;
    }

    public static CnssNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CNSS number cannot be empty", nameof(value));

        value = value.Trim();

        if (!IsValidCnssNumber(value))
            throw new ArgumentException($"Invalid CNSS number format: {value}", nameof(value));

        return new CnssNumber(value);
    }

    private static bool IsValidCnssNumber(string value)
    {
        // CNSS (Caisse Nationale de Sécurité Sociale) affiliation number
        // Typically 8-10 digits
        if (value.Length < 8 || value.Length > 10)
            return false;

        return value.All(char.IsDigit);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(CnssNumber cnss) => cnss.Value;
}