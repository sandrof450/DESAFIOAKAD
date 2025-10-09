using MicroServicoVendas.Clients;
using MicroServicoVendas.DTOs;
using MicroServicoVendas.Interfaces;
using MicroServicoVendas.Models;
using MicroServicoVendas.Repositories;
using MicroServicoVendas.Repositories.Context;
using System.Linq;

namespace MicroServicoVendas.Services
{
    public class PedidoService
    {
        private readonly PedidoRepository _pedidoRepository;
        private readonly EstoqueClient _estoqueClient;
        private readonly IRabbitMqPublisher _publisher;

        public PedidoService(
            VendaContext context,
            PedidoRepository pedidoRepository,
            EstoqueClient estoqueClient,
            IRabbitMqPublisher publisher)
        {
            _pedidoRepository = pedidoRepository;
            _estoqueClient = estoqueClient;
            _publisher = publisher;
        }

        public async Task<Pedido> CreatePedido(PedidoDTO pedidoDTO)
        {
            #region Validate Pedido is not null
            if (pedidoDTO == null)
                throw new ArgumentNullException(nameof(pedidoDTO), "Pedido is not can null");
            #endregion

            #region Add ProdutoId and information coming from the Micro Serviço estoque
            var produto = await _estoqueClient.GetProdutoAsNameAsync(pedidoDTO.NomeProduto);
            if (produto == null)
                throw new Exception($"No product found with the name {pedidoDTO.NomeProduto}.");
            #endregion

            #region Validate if there is sufficient quantity in Estoque; if so, deduct the quantity from Estoque
            var existsQuantidadeSuficiente = pedidoDTO.Quantidade <= produto.QuantidadeDisponivel;

            if (!existsQuantidadeSuficiente)
            {
                throw new Exception("Requested quantity is not available. The quantity in Estoque is less than requested.");
            }
            //Descontar quantidade no produto.
            var produtoId = produto.Id;
            produto.QuantidadeDisponivel -= pedidoDTO.Quantidade;
            await _estoqueClient.UpdateProdutoAsync(produtoId, produto);
            #endregion

            #region Publish message to RabbitMQ queue
            var message = $"Product {produto.NomeProduto} had {pedidoDTO.Quantidade} units removed.";
            _publisher.Publish("vendaNotification", message);
            #endregion

            #region Add the new Pedido to the context
            var pedido = new Pedido()
            {
                NomeProduto = pedidoDTO.NomeProduto,
                Preco = pedidoDTO.Preco,
                Quantidade = pedidoDTO.Quantidade,
                ProdutoId = produtoId,
            };

            var novoPedido = await _pedidoRepository.CreatePedido(pedido);
            #endregion

            return novoPedido;
        }

        public List<Pedido> GetPedidos()
        {
            #region Get Pedidos to the context
            var listaPedidos = _pedidoRepository.GetPedidos();
            #endregion

            #region Validate Pedido is not null
            if (listaPedidos == null)
                throw new ArgumentNullException(nameof(listaPedidos), "Pedido is not can null");
            #endregion

            return listaPedidos;
        }

        public Pedido GetPedido(int id)
        {
            #region Get Pedidos to the context
            var pedido = _pedidoRepository.GetPedido(id);
            #endregion

            #region Validate Pedido is not null
            if (pedido == null)
            {
                throw new Exception($"Pedido with id {id} not found.");
            }
            #endregion

            return pedido;
        }

        public async Task<Pedido> UpdatePedido(int id, Pedido pedido)
        {
            #region Validate Pedido is not null or Not exists
            var existsPedido = _pedidoRepository.GetPedido(id) != null;
            if (!existsPedido)
            {
                throw new Exception("Pedido not found");
            }

            if (pedido == null)
            {
                throw new Exception($"Pedido with id {id} not found.");
            }
            #endregion

            #region Update Pedido to the context
            var pedidoUpdate = await _pedidoRepository.UpdatePedido(id, pedido);
            #endregion

            return pedidoUpdate;
        }

        public void DeletePedido(int id)
        {
            #region Validate Pedido is not null or Not exists
            var existsPedido = _pedidoRepository.GetPedido(id) != null;
            if (!existsPedido)
            {
                throw new Exception("Pedido not found");
            }
            #endregion

            #region Update Pedido to the context
            _pedidoRepository.DeletePedido(id);
            #endregion

        }

        public bool GetExistsSingleProductByName(string name)
        {
            var pedidos = _pedidoRepository.GetPedidoAsName(name);
            var ExistsSingleProductByName = pedidos.Count() == 0;

            return ExistsSingleProductByName;
        }

    }
}