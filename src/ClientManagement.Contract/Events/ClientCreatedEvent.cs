namespace ClientManagement.Contract.Events;

public record ClientCreatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Source { get; init; } = "ClientManagementService";
    public string Name { get; init; } = string.Empty;
    public string Cif { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}