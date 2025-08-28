namespace ClientManagement.Domain.Entities;

public class ClientGroup
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Navigation properties
    public ICollection<ClientGroupMembership> ClientGroupMemberships { get; set; } = new List<ClientGroupMembership>();
}