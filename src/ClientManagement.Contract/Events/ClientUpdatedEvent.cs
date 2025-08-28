namespace ClientManagement.Contract.Events;

public record ClientUpdatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Source { get; init; } = "ClientManagementService";
    public string CompanyName { get; init; } = string.Empty;
    public string? Country { get; init; }
    public string? Address { get; init; }
    public string? IceNumber { get; init; }
    public string? RcNumber { get; init; }
    public string? VatNumber { get; init; }
    public string? CnssNumber { get; init; }
    public string? Industry { get; init; }
    public string? AdminContactPerson { get; init; }
    public string? BillingContactPerson { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? FiscalYearEnd { get; init; }
    public string? AssignedTeamId { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string UpdatedBy { get; init; } = string.Empty;
}