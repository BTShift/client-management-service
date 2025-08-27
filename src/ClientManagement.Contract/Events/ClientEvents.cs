namespace ClientManagement.Contract.Events;

public record ClientCreatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string Industry { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string IceNumber { get; init; } = string.Empty;
    public string RcNumber { get; init; } = string.Empty;
}

public record ClientUpdatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public Dictionary<string, object> Changes { get; init; } = new();
}

public record ClientDeletedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
}

public record ClientActivatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string PreviousStatus { get; init; } = string.Empty;
}

public record ClientDeactivatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record ClientGroupCreatedEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string GroupId { get; init; } = string.Empty;
    public string GroupName { get; init; } = string.Empty;
}

public record ClientAddedToGroupEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string GroupId { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string GroupName { get; init; } = string.Empty;
}

public record ClientRemovedFromGroupEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string GroupId { get; init; } = string.Empty;
}

public record UserAssignedToClientEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

public record UserRemovedFromClientEvent : IBaseEvent
{
    public Guid CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public string TenantId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
}