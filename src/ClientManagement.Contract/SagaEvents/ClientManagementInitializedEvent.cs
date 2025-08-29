namespace ClientManagement.Contract.SagaEvents;

public record ClientManagementInitializedEvent
{
    public Guid CorrelationId { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public bool SchemaCreated { get; init; }
    public DateTime InitializedAt { get; init; }
}