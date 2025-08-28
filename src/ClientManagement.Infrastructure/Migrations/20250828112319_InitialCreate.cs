using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "client_management");

            migrationBuilder.CreateTable(
                name: "client_groups",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    ice_number = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    rc_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vat_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cnss_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    admin_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    billing_contact_person = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    fiscal_year_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assigned_team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "client_group_memberships",
                schema: "client_management",
                columns: table => new
                {
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    added_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_group_memberships", x => new { x.client_id, x.group_id });
                    table.ForeignKey(
                        name: "FK_client_group_memberships_client_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "client_management",
                        principalTable: "client_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_client_group_memberships_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "client_management",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_client_associations",
                schema: "client_management",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    assigned_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_client_associations", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_client_associations_clients_client_id",
                        column: x => x.client_id,
                        principalSchema: "client_management",
                        principalTable: "clients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_client_group_memberships_client_id",
                schema: "client_management",
                table: "client_group_memberships",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_client_group_memberships_group_id",
                schema: "client_management",
                table: "client_group_memberships",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_client_groups_tenant_deleted",
                schema: "client_management",
                table: "client_groups",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "ix_client_groups_tenant_id",
                schema: "client_management",
                table: "client_groups",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_client_groups_tenant_name",
                schema: "client_management",
                table: "client_groups",
                columns: new[] { "tenant_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clients_assigned_team_id",
                schema: "client_management",
                table: "clients",
                column: "assigned_team_id",
                filter: "assigned_team_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_cnss_number",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "cnss_number" },
                filter: "cnss_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_company_name",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "company_name" });

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_deleted",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_deletion_audit",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "deleted_at", "deleted_by" },
                filter: "deleted_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_ice_number",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "ice_number" },
                filter: "ice_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_id",
                schema: "client_management",
                table: "clients",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_rc_number",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "rc_number" },
                filter: "rc_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_status",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_vat_number",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "vat_number" },
                filter: "vat_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_user_client_associations_client",
                schema: "client_management",
                table: "user_client_associations",
                columns: new[] { "tenant_id", "client_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_client_associations_client_id",
                schema: "client_management",
                table: "user_client_associations",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_client_associations_unique",
                schema: "client_management",
                table: "user_client_associations",
                columns: new[] { "tenant_id", "user_id", "client_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_client_associations_user",
                schema: "client_management",
                table: "user_client_associations",
                columns: new[] { "tenant_id", "user_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_group_memberships",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "user_client_associations",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "client_groups",
                schema: "client_management");

            migrationBuilder.DropTable(
                name: "clients",
                schema: "client_management");
        }
    }
}
