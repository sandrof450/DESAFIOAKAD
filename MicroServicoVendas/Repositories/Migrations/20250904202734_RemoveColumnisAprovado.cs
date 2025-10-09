using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroServicoVendas.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveColumnisAprovado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAprovado",
                table: "Pedidos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAprovado",
                table: "Pedidos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
