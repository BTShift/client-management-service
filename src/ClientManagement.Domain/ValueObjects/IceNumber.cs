namespace ClientManagement.Domain.ValueObjects;

public class IceNumber : ValueObject
{
    public string Value { get; }

    private IceNumber(string value)
    {
        Value = value;
    }

    public static IceNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ICE number cannot be empty", nameof(value));

        value = value.Trim();

        if (!IsValidIceNumber(value))
            throw new ArgumentException($"Invalid ICE number format: {value}", nameof(value));

        return new IceNumber(value);
    }

    private static bool IsValidIceNumber(string value)
    {
        // ICE (Identifiant Commun de l'Entreprise) in Morocco is a 15-digit number
        if (value.Length != 15)
            return false;

        return value.All(char.IsDigit);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(IceNumber ice) => ice.Value;
}