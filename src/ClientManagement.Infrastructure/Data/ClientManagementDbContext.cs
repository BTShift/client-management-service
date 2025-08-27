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
            
            entity.Property(e => e.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(250);
            
            entity.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasMaxLength(50);
            
            entity.Property(e => e.Address)
                .HasColumnName("address")
                .HasMaxLength(500);
            
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(20);
            
            entity.Property(e => e.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            
            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
            
            entity.HasIndex(e => e.TenantId)
                .HasDatabaseName("ix_clients_tenant_id");
            
            entity.HasIndex(e => new { e.TenantId, e.Email })
                .HasDatabaseName("ix_clients_tenant_email")
                .IsUnique();
            
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}