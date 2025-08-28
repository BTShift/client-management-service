using Microsoft.EntityFrameworkCore;
using ClientManagement.Domain.Entities;

namespace ClientManagement.Infrastructure.Data;

public class ClientManagementDbContext : DbContext
{
    public ClientManagementDbContext(DbContextOptions<ClientManagementDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientGroup> ClientGroups { get; set; }
    public DbSet<ClientGroupMembership> ClientGroupMemberships { get; set; }
    public DbSet<UserClientAssociation> UserClientAssociations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.HasDefaultSchema("client_management");
        
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.CompanyName)
                .HasColumnName("company_name")
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Country)
                .HasColumnName("country")
                .HasMaxLength(100);
            
            entity.Property(e => e.Address)
                .HasColumnName("address");
            
            entity.Property(e => e.IceNumber)
                .HasColumnName("ice_number")
                .HasMaxLength(15);
            
            entity.Property(e => e.RcNumber)
                .HasColumnName("rc_number")
                .HasMaxLength(50);
            
            entity.Property(e => e.VatNumber)
                .HasColumnName("vat_number")
                .HasMaxLength(50);
            
            entity.Property(e => e.CnssNumber)
                .HasColumnName("cnss_number")
                .HasMaxLength(50);
            
            entity.Property(e => e.Industry)
                .HasColumnName("industry")
                .HasMaxLength(100);
            
            entity.Property(e => e.AdminContactPerson)
                .HasColumnName("admin_contact_person")
                .HasMaxLength(200);
            
            entity.Property(e => e.BillingContactPerson)
                .HasColumnName("billing_contact_person")
                .HasMaxLength(200);
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ClientStatus.Active);
            
            entity.Property(e => e.FiscalYearEnd)
                .HasColumnName("fiscal_year_end");
            
            entity.Property(e => e.AssignedTeamId)
                .HasColumnName("assigned_team_id");
            
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            
            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
            
            entity.Property(e => e.DeletedBy)
                .HasColumnName("deleted_by")
                .HasMaxLength(100);
            
            // Indexes for better query performance
            entity.HasIndex(e => e.TenantId)
                .HasDatabaseName("ix_clients_tenant_id");
            
            entity.HasIndex(e => new { e.TenantId, e.CompanyName })
                .HasDatabaseName("ix_clients_tenant_company_name");
            
            entity.HasIndex(e => new { e.TenantId, e.IceNumber })
                .HasDatabaseName("ix_clients_tenant_ice_number")
                .HasFilter("ice_number IS NOT NULL");
            
            entity.HasIndex(e => new { e.TenantId, e.RcNumber })
                .HasDatabaseName("ix_clients_tenant_rc_number")
                .HasFilter("rc_number IS NOT NULL");
            
            entity.HasIndex(e => new { e.TenantId, e.VatNumber })
                .HasDatabaseName("ix_clients_tenant_vat_number")
                .HasFilter("vat_number IS NOT NULL");
            
            entity.HasIndex(e => new { e.TenantId, e.CnssNumber })
                .HasDatabaseName("ix_clients_tenant_cnss_number")
                .HasFilter("cnss_number IS NOT NULL");
            
            entity.HasIndex(e => new { e.TenantId, e.Status })
                .HasDatabaseName("ix_clients_tenant_status");
            
            entity.HasIndex(e => new { e.TenantId, e.IsDeleted })
                .HasDatabaseName("ix_clients_tenant_deleted");
            
            entity.HasIndex(e => e.AssignedTeamId)
                .HasDatabaseName("ix_clients_assigned_team_id")
                .HasFilter("assigned_team_id IS NOT NULL");
            
            // Audit index for tracking deletions
            entity.HasIndex(e => new { e.TenantId, e.DeletedAt, e.DeletedBy })
                .HasDatabaseName("ix_clients_tenant_deletion_audit")
                .HasFilter("deleted_at IS NOT NULL");
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // ClientGroup configuration
        modelBuilder.Entity<ClientGroup>(entity =>
        {
            entity.ToTable("client_groups");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);
            
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            
            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
            
            entity.Property(e => e.DeletedBy)
                .HasColumnName("deleted_by")
                .HasMaxLength(100);
            
            // Indexes
            entity.HasIndex(e => e.TenantId)
                .HasDatabaseName("ix_client_groups_tenant_id");
            
            entity.HasIndex(e => new { e.TenantId, e.Name })
                .HasDatabaseName("ix_client_groups_tenant_name")
                .IsUnique();
            
            entity.HasIndex(e => new { e.TenantId, e.IsDeleted })
                .HasDatabaseName("ix_client_groups_tenant_deleted");
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
        
        // ClientGroupMembership configuration (many-to-many relationship)
        modelBuilder.Entity<ClientGroupMembership>(entity =>
        {
            entity.ToTable("client_group_memberships");
            
            entity.HasKey(e => new { e.ClientId, e.GroupId });
            
            entity.Property(e => e.ClientId)
                .HasColumnName("client_id");
            
            entity.Property(e => e.GroupId)
                .HasColumnName("group_id");
            
            entity.Property(e => e.JoinedAt)
                .HasColumnName("joined_at");
            
            entity.Property(e => e.AddedBy)
                .HasColumnName("added_by")
                .HasMaxLength(100);
            
            // Relationships
            entity.HasOne(e => e.Client)
                .WithMany(c => c.ClientGroupMemberships)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Group)
                .WithMany(g => g.ClientGroupMemberships)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => e.ClientId)
                .HasDatabaseName("ix_client_group_memberships_client_id");
            
            entity.HasIndex(e => e.GroupId)
                .HasDatabaseName("ix_client_group_memberships_group_id");
        });
        
        // UserClientAssociation configuration
        modelBuilder.Entity<UserClientAssociation>(entity =>
        {
            entity.ToTable("user_client_associations");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.ClientId)
                .HasColumnName("client_id");
            
            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.AssignedAt)
                .HasColumnName("assigned_at");
            
            entity.Property(e => e.AssignedBy)
                .HasColumnName("assigned_by")
                .IsRequired()
                .HasMaxLength(100);
            
            // Relationships
            entity.HasOne(e => e.Client)
                .WithMany(c => c.UserAssociations)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes
            entity.HasIndex(e => new { e.TenantId, e.UserId, e.ClientId })
                .HasDatabaseName("ix_user_client_associations_unique")
                .IsUnique();
            
            entity.HasIndex(e => new { e.TenantId, e.UserId })
                .HasDatabaseName("ix_user_client_associations_user");
            
            entity.HasIndex(e => new { e.TenantId, e.ClientId })
                .HasDatabaseName("ix_user_client_associations_client");
        });
    }
}