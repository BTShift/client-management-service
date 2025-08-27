using ClientManagement.Domain.Common;

namespace ClientManagement.Domain.Entities;

public class Department : Entity
{
    private readonly List<string> _teamIds = new();

    public string ClientId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    
    public IReadOnlyCollection<string> TeamIds => _teamIds.AsReadOnly();

    private Department(
        string id,
        string clientId,
        string name,
        string? description) : base(id)
    {
        ClientId = clientId;
        Name = name;
        Description = description;
    }

    public static Department Create(
        string id,
        string clientId,
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Department ID cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be empty", nameof(clientId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name cannot be empty", nameof(name));

        return new Department(
            id,
            clientId.Trim(),
            name.Trim(),
            description?.Trim()
        );
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name cannot be empty", nameof(name));

        Name = name.Trim();
        UpdateTimestamp();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdateTimestamp();
    }

    public void AddTeam(string teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId))
            throw new ArgumentException("Team ID cannot be empty", nameof(teamId));

        if (!_teamIds.Contains(teamId))
        {
            _teamIds.Add(teamId);
            UpdateTimestamp();
        }
    }

    public void RemoveTeam(string teamId)
    {
        if (_teamIds.Remove(teamId))
        {
            UpdateTimestamp();
        }
    }

    public bool ContainsTeam(string teamId)
    {
        return _teamIds.Contains(teamId);
    }

    public void ClearTeams()
    {
        if (_teamIds.Count > 0)
        {
            _teamIds.Clear();
            UpdateTimestamp();
        }
    }
}