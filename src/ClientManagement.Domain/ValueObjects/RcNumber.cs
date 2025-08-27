namespace ClientManagement.Domain.ValueObjects;

public class RcNumber : ValueObject
{
    public string Value { get; }

    private RcNumber(string value)
    {
        Value = value;
    }

    public static RcNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RC number cannot be empty", nameof(value));

        value = value.Trim().ToUpper();

        if (!IsValidRcNumber(value))
            throw new ArgumentException($"Invalid RC number format: {value}", nameof(value));

        return new RcNumber(value);
    }

    private static bool IsValidRcNumber(string value)
    {
        // RC (Registre de Commerce) format varies by Moroccan city
        // Generally contains numbers and sometimes letters
        // Minimum length is typically 4 characters
        if (value.Length < 4 || value.Length > 20)
            return false;

        // Must contain at least one digit
        return value.Any(char.IsDigit);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(RcNumber rc) => rc.Value;
}