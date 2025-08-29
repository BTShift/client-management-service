using ClientManagement.Contract.Events;

namespace ClientManagement.Contract.SagaEvents;

public record InitializeClientManagementCommand : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string TenantId { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = string.Empty;
}