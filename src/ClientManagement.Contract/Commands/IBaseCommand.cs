namespace ClientManagement.Contract.Commands;

public interface IBaseCommand
{
    Guid CorrelationId { get; init; }
}