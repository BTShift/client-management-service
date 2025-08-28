namespace ClientManagement.Contract.Events;

public record GroupDeletedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string GroupId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Source { get; init; } = "ClientManagementService";
    public DateTime DeletedAt { get; init; }
    public string DeletedBy { get; init; } = string.Empty;
}