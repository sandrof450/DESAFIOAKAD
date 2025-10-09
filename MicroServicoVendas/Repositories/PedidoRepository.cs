using MicroServicoVendas.DTOs;
using MicroServicoVendas.Models;
using MicroServicoVendas.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MicroServicoVendas.Repositories
{
    public class PedidoRepository
    {
        private readonly VendaContext _context;

        public PedidoRepository(VendaContext context)
        {
            _context = context;
        }

        public async Task<Pedido> CreatePedido(Pedido pedido)
        {
            var novoPedido = await _context.Pedidos.AddAsync(pedido);

            var validateIfSaveChanges = _context.SaveChanges() > 0;

            if (!validateIfSaveChanges)
            {
                throw new Exception("Failed to save Pedido.");
            }

            return novoPedido.Entity;
        }

        public List<Pedido> GetPedidos()
        {
            var listaPedidos = _context.Pedidos.AsNoTracking().ToList();

            return listaPedidos;
        }

        public Pedido GetPedido(int id)
        {
            var pedido = _context.Pedidos.AsNoTracking().FirstOrDefault(p => p.Id == id);

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

        public void DeletePedido(int id)
        {
            var pedidoDeleted = GetPedido(id);
            _context.Pedidos.Remove(pedidoDeleted);

            var validateIfSaveChanges = _context.SaveChanges() > 0;

            if (!validateIfSaveChanges)
            {
                throw new Exception("Failed to save Pedido.");
            }
            
        }
    }
}