using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserClientAssociations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "user_client_associations",
                schema: "client_management");
        }
    }
}
