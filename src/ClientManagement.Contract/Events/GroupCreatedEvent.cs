namespace ClientManagement.Contract.Events;

public record GroupCreatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string GroupId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Source { get; init; } = "ClientManagementService";
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}