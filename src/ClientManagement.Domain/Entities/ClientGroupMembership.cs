namespace ClientManagement.Domain.Entities;

public class ClientGroupMembership
{
    public Guid ClientId { get; set; }
    public Guid GroupId { get; set; }
    public DateTime JoinedAt { get; set; }
    public string? AddedBy { get; set; }
    
    // Navigation properties
    public Client Client { get; set; } = null!;
    public ClientGroup Group { get; set; } = null!;
}