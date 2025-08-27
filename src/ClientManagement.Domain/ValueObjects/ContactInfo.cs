namespace ClientManagement.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public string Name { get; }
    public string Email { get; }
    public string? Phone { get; }
    public string? Title { get; }

    private ContactInfo(string name, string email, string? phone, string? title)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Title = title;
    }

    public static ContactInfo Create(string name, string email, string? phone = null, string? title = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Contact name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Contact email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException($"Invalid email format: {email}", nameof(email));

        if (phone != null && !IsValidPhone(phone))
            throw new ArgumentException($"Invalid phone format: {phone}", nameof(phone));

        return new ContactInfo(
            name.Trim(),
            email.Trim().ToLowerInvariant(),
            phone?.Trim(),
            title?.Trim()
        );
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhone(string phone)
    {
        // Basic phone validation - can be enhanced for specific formats
        phone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
        return phone.Length >= 8 && phone.Length <= 15 && phone.All(char.IsDigit);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Email;
        yield return Phone ?? string.Empty;
        yield return Title ?? string.Empty;
    }

    public override string ToString()
    {
        var result = $"{Name} ({Email})";
        if (!string.IsNullOrWhiteSpace(Title))
            result = $"{Title} - {result}";
        if (!string.IsNullOrWhiteSpace(Phone))
            result += $" - {Phone}";
        return result;
    }
}