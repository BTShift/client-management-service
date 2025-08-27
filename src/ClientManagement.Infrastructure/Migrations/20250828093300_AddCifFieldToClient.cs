using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCifFieldToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cif",
                schema: "client_management",
                table: "clients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_clients_tenant_cif",
                schema: "client_management",
                table: "clients",
                columns: new[] { "tenant_id", "cif" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_clients_tenant_cif",
                schema: "client_management",
                table: "clients");

            migrationBuilder.DropColumn(
                name: "cif",
                schema: "client_management",
                table: "clients");
        }
    }
}
