namespace ClientManagement.Contract.Commands;

public record InitializeClientManagementCommand
{
    public Guid CorrelationId { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = string.Empty;
}