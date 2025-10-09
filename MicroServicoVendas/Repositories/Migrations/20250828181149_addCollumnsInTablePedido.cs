using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroServicoVendas.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addCollumnsInTablePedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAprovado",
                table: "Pedidos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NomeProduto",
                table: "Pedidos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAprovado",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "NomeProduto",
                table: "Pedidos");
        }
    }
}
