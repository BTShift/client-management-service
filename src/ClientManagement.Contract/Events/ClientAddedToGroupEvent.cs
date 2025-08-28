namespace ClientManagement.Contract.Events;

public record ClientAddedToGroupEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public string GroupId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Source { get; init; } = "ClientManagementService";
    public string ClientName { get; init; } = string.Empty;
    public string ClientCif { get; init; } = string.Empty;
    public string GroupName { get; init; } = string.Empty;
    public DateTime AddedAt { get; init; }
    public string AddedBy { get; init; } = string.Empty;
}