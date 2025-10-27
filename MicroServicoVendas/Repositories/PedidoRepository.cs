using MicroServicoVendas.DTOs;
using MicroServicoVendas.Models;
using MicroServicoVendas.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoVendas.Repositories
{
    public class PedidoRepository
    {
        private readonly VendaContext _context;

        public PedidoRepository(VendaContext context)
        {
            _context = context;
        }

        public async Task<Pedido> CreatePedidoAsync(Pedido pedido)
        {
            var novoPedido = await _context.Pedidos.AddAsync(pedido);

            var validateIfSaveChanges = await _context.SaveChangesAsync() > 0;

            if (!validateIfSaveChanges)
            {
                throw new Exception("Failed to save Pedido.");
            }

            return novoPedido.Entity;
        }

        public async Task<List<Pedido>> GetPedidosAsync()
        {
            var listaPedidos = await _context.Pedidos.AsNoTracking().ToListAsync();

            return listaPedidos;
        }

        public async Task<Pedido> GetPedidoAsync(int id)
        {
            var pedido = await _context.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            return pedido;
        }

        public List<Pedido> GetPedidoAsName(string name)
        {
            var pedido =
            (
                from p in _context.Pedidos
                where p.NomeProduto.Contains(name)
                select p
            )
            .AsNoTracking()
            .ToList();

            return pedido;
        }

        public async Task<List<int>> GetListCountVendasMensaisByProdutoAsync(int produtoId)
        {
            
            var pedidos = await
            (
                from p in _context.Pedidos
                where p.ProdutoId == produtoId
                select p
            )
            .AsNoTracking()
            .ToListAsync();

            var listCountMensalProduto = pedidos
                .GroupBy(p => new { Year = p.DataPedido.Year, Month = p.DataPedido.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => g.Sum(p => p.Quantidade))
                .ToList();
            return listCountMensalProduto;
        }
        public async Task<Pedido> UpdatePedido(int id, Pedido pedido)
        {
            var pedidoUpdate = await _context.Pedidos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            pedidoUpdate.NomeProduto = pedido.NomeProduto;
            pedidoUpdate.Preco = pedido.Preco;
            pedidoUpdate.Quantidade = pedido.Quantidade;

            _context.Pedidos.Update(pedidoUpdate);

            var validateIfSaveChanges = _context.SaveChanges() > 0;

            if (!validateIfSaveChanges)
            {
                throw new Exception("Failed to save Pedido.");
            }

            return pedidoUpdate;
        }

        public async Task DeletePedidoAsync(int id)
        {
            var pedidoDeleted = await GetPedidoAsync(id);

            if (pedidoDeleted == null)
            {
                throw new Exception("Pedido not found.");
            }

            _context.Pedidos.Remove(pedidoDeleted);

            var validateIfSaveChanges = await _context.SaveChangesAsync() > 0;

            if (!validateIfSaveChanges)
            {
                throw new Exception("Failed to save Pedido.");
            }
        }
    }
}