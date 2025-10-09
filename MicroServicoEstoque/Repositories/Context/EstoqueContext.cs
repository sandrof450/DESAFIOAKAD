
using MicroServicoEstoque.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroServicoEstoque.Repositories.Context
{
    public class EstoqueContext : DbContext
    {
        public EstoqueContext(DbContextOptions<EstoqueContext> options) : base(options) { }

        public DbSet<Produto> Produtos { get; set; }
    }
}