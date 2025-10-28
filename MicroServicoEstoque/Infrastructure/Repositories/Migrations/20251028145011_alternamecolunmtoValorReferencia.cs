using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MicroServicoEstoque.Infrastructure.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class alternamecolunmtoValorReferencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Preco",
                table: "Produtos",
                newName: "ValorReferencia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValorReferencia",
                table: "Produtos",
                newName: "Preco");
        }
    }
}
