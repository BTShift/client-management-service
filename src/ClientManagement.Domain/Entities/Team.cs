using ClientManagement.Domain.Common;

namespace ClientManagement.Domain.Entities;

public class Team : Entity
{
    private readonly List<string> _userIds = new();

    public string DepartmentId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    
    public IReadOnlyCollection<string> UserIds => _userIds.AsReadOnly();

    private Team(
        string id,
        string departmentId,
        string name,
        string? description) : base(id)
    {
        DepartmentId = departmentId;
        Name = name;
        Description = description;
    }

    public static Team Create(
        string id,
        string departmentId,
        string name,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Team ID cannot be empty", nameof(id));

        if (string.IsNullOrWhiteSpace(departmentId))
            throw new ArgumentException("Department ID cannot be empty", nameof(departmentId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be empty", nameof(name));

        return new Team(
            id,
            departmentId.Trim(),
            name.Trim(),
            description?.Trim()
        );
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Team name cannot be empty", nameof(name));

        Name = name.Trim();
        UpdateTimestamp();
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdateTimestamp();
    }

    public void AssignUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (!_userIds.Contains(userId))
        {
            _userIds.Add(userId);
            UpdateTimestamp();
        }
    }

    public void RemoveUser(string userId)
    {
        if (_userIds.Remove(userId))
        {
            UpdateTimestamp();
        }
    }

    public bool ContainsUser(string userId)
    {
        return _userIds.Contains(userId);
    }

    public void ClearUsers()
    {
        if (_userIds.Count > 0)
        {
            _userIds.Clear();
            UpdateTimestamp();
        }
    }
}