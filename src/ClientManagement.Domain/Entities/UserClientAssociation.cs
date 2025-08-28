namespace ClientManagement.Domain.Entities;

public class UserClientAssociation
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty; // User ID from Identity service
    public Guid ClientId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public string AssignedBy { get; set; } = string.Empty; // User ID who made the assignment
    
    // Navigation properties
    public Client? Client { get; set; }
}