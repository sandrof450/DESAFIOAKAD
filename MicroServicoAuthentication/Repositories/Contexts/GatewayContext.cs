using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace APIGateway.Repositories.Contexts
{
    public class GatewayContext : DbContext
    {
        public GatewayContext(DbContextOptions<GatewayContext> options) : base(options) { }

        public DbSet<Models.Administrador> Administradores { get; set; }
        
    }
}