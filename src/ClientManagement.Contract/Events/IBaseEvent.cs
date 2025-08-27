namespace ClientManagement.Contract.Events;

public interface IBaseEvent
{
    Guid CorrelationId { get; init; }
    DateTime Timestamp { get; init; }
    string TenantId { get; init; }
}