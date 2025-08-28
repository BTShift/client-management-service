namespace ClientManagement.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Address { get; set; }
    public string? IceNumber { get; set; } // Identifiant Commun de l'Entreprise
    public string? RcNumber { get; set; } // Registre de Commerce number
    public string? VatNumber { get; set; } // VAT registration number
    public string? CnssNumber { get; set; } // Caisse Nationale de Sécurité Sociale number
    public string? Industry { get; set; }
    public string? AdminContactPerson { get; set; }
    public string? BillingContactPerson { get; set; }
    public ClientStatus Status { get; set; } = ClientStatus.Active;
    public DateTime? FiscalYearEnd { get; set; }
    public Guid? AssignedTeamId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; } // User ID or email who performed soft delete
    
    // Navigation properties
    public ICollection<ClientGroupMembership> ClientGroupMemberships { get; set; } = new List<ClientGroupMembership>();
    public ICollection<UserClientAssociation> UserAssociations { get; set; } = new List<UserClientAssociation>();
}

public enum ClientStatus
{
    Active,
    Inactive,
    Suspended
}