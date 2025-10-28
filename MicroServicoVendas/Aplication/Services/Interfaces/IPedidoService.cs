using MicroServicoVendas.Aplication.DTOs;
using MicroServicoVendas.Domain.Models;

namespace MicroServicoVendas.Aplication.Services.Interfaces
{
    public interface IPedidoService
    {
        Task<Pedido> CreatePedidoAsync(PedidoDTO pedidoDTO);
        Task<List<Pedido>> GetPedidosAsync();
        Task<Pedido> GetPedidoAsync(int id);
        Task<List<int>> GetListCountVendasMensaisByProdutoAsync(int produtoId);
        Task<Pedido> UpdatePedidoAsync(int id, Pedido pedido);
        Task DeletePedidoAsync(int id);
        bool GetExistsSingleProductByName(string name);
    }
}