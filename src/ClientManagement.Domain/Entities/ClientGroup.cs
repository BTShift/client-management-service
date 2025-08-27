using ClientManagement.Domain.Common;

namespace ClientManagement.Domain.Entities;

public class ClientGroup : Entity
{
    private readonly List<string> _clientIds = new();

    public string TenantId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    
    public IReadOnlyCollection<string> ClientIds => _clientIds.AsReadOnly();

    private ClientGroup(
        string id,
        string tenantId,
        string name,
        string? description) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
    }

    public static ClientGroup Create(
        string id,
        string tenantId,
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Group ID cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty", nameof(name));

        return new ClientGroup(
            id,
            tenantId.Trim(),
            name.Trim(),
            description?.Trim()
        );
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be empty", nameof(name));

        Name = name.Trim();
        UpdateTimestamp();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdateTimestamp();
    }

    public void AddClient(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be empty", nameof(clientId));

        if (!_clientIds.Contains(clientId))
        {
            _clientIds.Add(clientId);
            UpdateTimestamp();
        }
    }

    public void RemoveClient(string clientId)
    {
        if (_clientIds.Remove(clientId))
        {
            UpdateTimestamp();
        }
    }

    public bool ContainsClient(string clientId)
    {
        return _clientIds.Contains(clientId);
    }

    public void ClearClients()
    {
        if (_clientIds.Count > 0)
        {
            _clientIds.Clear();
            UpdateTimestamp();
        }
    }
}