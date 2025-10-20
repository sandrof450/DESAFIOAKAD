
using MicroServicoVendas.DTOs;

namespace MicroServicoVendas.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public string NomeProduto { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataPedido { get; set; }
        public int ProdutoId { get; set; } = 0;
    }
}