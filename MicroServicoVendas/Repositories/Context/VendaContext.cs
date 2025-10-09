

using MicroServicoVendas.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroServicoVendas.Repositories.Context
{
    public class VendaContext : DbContext
    {
        public VendaContext(DbContextOptions<VendaContext> options) : base(options) { }

        public DbSet<Pedido> Pedidos { get; set; }
    }
}