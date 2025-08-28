namespace ClientManagement.Contract.Events;

public record GroupUpdatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string GroupId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Source { get; init; } = "ClientManagementService";
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime UpdatedAt { get; init; }
    public string UpdatedBy { get; init; } = string.Empty;
}