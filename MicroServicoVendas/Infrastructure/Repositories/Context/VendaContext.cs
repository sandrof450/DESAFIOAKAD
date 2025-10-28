

using MicroServicoVendas.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroServicoVendas.Infrastructure.Repositories.Context
{
    public class VendaContext : DbContext
    {
        public VendaContext(DbContextOptions<VendaContext> options) : base(options) { }

        public DbSet<Pedido> Pedidos { get; set; }
    }
}