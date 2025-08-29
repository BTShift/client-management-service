using ClientManagement.Contract.Events;

namespace ClientManagement.Contract.Events;

public record ClientManagementInitializedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public bool SchemaCreated { get; init; }
    public DateTime InitializedAt { get; init; } = DateTime.UtcNow;
}