using ClientManagement.Domain.Common;
using ClientManagement.Domain.ValueObjects;

namespace ClientManagement.Domain.Entities;

public class Client : Entity
{
    private readonly List<string> _userIds = new();
    private readonly List<string> _groupIds = new();

    public string TenantId { get; private set; }
    public string CompanyName { get; private set; }
    public string Country { get; private set; }
    public Address Address { get; private set; }
    public IceNumber? IceNumber { get; private set; }
    public RcNumber? RcNumber { get; private set; }
    public VatNumber? VatNumber { get; private set; }
    public CnssNumber? CnssNumber { get; private set; }
    public Industry Industry { get; private set; }
    public ContactInfo AdminContact { get; private set; }
    public ContactInfo? BillingContact { get; private set; }
    public ClientStatus Status { get; private set; }
    public int FiscalYearEndMonth { get; private set; }
    
    public IReadOnlyCollection<string> UserIds => _userIds.AsReadOnly();
    public IReadOnlyCollection<string> GroupIds => _groupIds.AsReadOnly();

    private Client(
        string id,
        string tenantId,
        string companyName,
        string country,
        Address address,
        Industry industry,
        ContactInfo adminContact,
        ClientStatus status,
        int fiscalYearEndMonth) : base(id)
    {
        TenantId = tenantId;
        CompanyName = companyName;
        Country = country;
        Address = address;
        Industry = industry;
        AdminContact = adminContact;
        Status = status;
        FiscalYearEndMonth = fiscalYearEndMonth;
    }

    public static Client Create(
        string id,
        string tenantId,
        string companyName,
        string country,
        Address address,
        Industry industry,
        ContactInfo adminContact,
        ClientStatus status = ClientStatus.Prospect,
        int fiscalYearEndMonth = 12)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Client ID cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be empty", nameof(companyName));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        if (address == null)
            throw new ArgumentNullException(nameof(address));

        if (adminContact == null)
            throw new ArgumentNullException(nameof(adminContact));

        if (fiscalYearEndMonth < 1 || fiscalYearEndMonth > 12)
            throw new ArgumentException("Fiscal year end month must be between 1 and 12", nameof(fiscalYearEndMonth));

        return new Client(
            id,
            tenantId.Trim(),
            companyName.Trim(),
            country.Trim(),
            address,
            industry,
            adminContact,
            status,
            fiscalYearEndMonth
        );
    }

    public void UpdateCompanyName(string companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Company name cannot be empty", nameof(companyName));

        CompanyName = companyName.Trim();
        UpdateTimestamp();
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        UpdateTimestamp();
    }

    public void SetMoroccanIdentifiers(
        IceNumber? ice = null,
        RcNumber? rc = null,
        VatNumber? vat = null,
        CnssNumber? cnss = null)
    {
        if (ice != null) IceNumber = ice;
        if (rc != null) RcNumber = rc;
        if (vat != null) VatNumber = vat;
        if (cnss != null) CnssNumber = cnss;
        UpdateTimestamp();
    }

    public void UpdateAdminContact(ContactInfo contact)
    {
        AdminContact = contact ?? throw new ArgumentNullException(nameof(contact));
        UpdateTimestamp();
    }

    public void UpdateBillingContact(ContactInfo? contact)
    {
        BillingContact = contact;
        UpdateTimestamp();
    }

    public void UpdateStatus(ClientStatus status)
    {
        if (status == ClientStatus.Unspecified)
            throw new ArgumentException("Cannot set status to unspecified", nameof(status));

        Status = status;
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (Status == ClientStatus.Active)
            throw new InvalidOperationException("Client is already active");

        Status = ClientStatus.Active;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (Status == ClientStatus.Inactive)
            throw new InvalidOperationException("Client is already inactive");

        Status = ClientStatus.Inactive;
        UpdateTimestamp();
    }

    public void UpdateFiscalYearEndMonth(int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentException("Fiscal year end month must be between 1 and 12", nameof(month));

        FiscalYearEndMonth = month;
        UpdateTimestamp();
    }

    public void AssignUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (!_userIds.Contains(userId))
        {
            _userIds.Add(userId);
            UpdateTimestamp();
        }
    }

    public void RemoveUser(string userId)
    {
        if (_userIds.Remove(userId))
        {
            UpdateTimestamp();
        }
    }

    public void AddToGroup(string groupId)
    {
        if (string.IsNullOrWhiteSpace(groupId))
            throw new ArgumentException("Group ID cannot be empty", nameof(groupId));

        if (!_groupIds.Contains(groupId))
        {
            _groupIds.Add(groupId);
            UpdateTimestamp();
        }
    }

    public void RemoveFromGroup(string groupId)
    {
        if (_groupIds.Remove(groupId))
        {
            UpdateTimestamp();
        }
    }
}

public enum ClientStatus
{
    Unspecified = 0,
    Prospect = 1,
    Active = 2,
    Inactive = 3
}

public enum Industry
{
    Unspecified = 0,
    Agriculture = 1,
    Manufacturing = 2,
    Services = 3,
    Retail = 4,
    Technology = 5,
    Finance = 6,
    Healthcare = 7,
    Education = 8,
    Other = 9
}