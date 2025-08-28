namespace ClientManagement.Contract.Events;

public record UserAssignedToClientEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Source { get; init; } = "ClientManagementService";
    public string Role { get; init; } = string.Empty;
    public DateTime AssignedAt { get; init; }
    public string AssignedBy { get; init; } = string.Empty;
}