using MicroServicoVendas.Domain.Models;

namespace MicroServicoVendas.Domain.Interfaces
{
    public interface IPedidoRepository
    {
        Task<Pedido> CreatePedidoAsync(Pedido pedido);
        Task<List<Pedido>> GetPedidosAsync();
        Task<Pedido> GetPedidoAsync(int id);
        List<Pedido> GetPedidoAsName(string name);
        Task<List<int>> GetListCountVendasMensaisByProdutoAsync(int produtoId);
        Task<Pedido> UpdatePedido(int id, Pedido pedido);
        Task DeletePedidoAsync(int id);
    }
}