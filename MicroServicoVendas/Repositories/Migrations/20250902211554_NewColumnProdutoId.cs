using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroServicoVendas.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class NewColumnProdutoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "Pedidos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "Pedidos");
        }
    }
}
