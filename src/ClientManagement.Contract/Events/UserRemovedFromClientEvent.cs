namespace ClientManagement.Contract.Events;

public record UserRemovedFromClientEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Source { get; init; } = "ClientManagementService";
    public DateTime RemovedAt { get; init; }
    public string RemovedBy { get; init; } = string.Empty;
}