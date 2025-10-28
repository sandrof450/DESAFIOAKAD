

namespace MicroServicoEstoque.Domain.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string NomeProduto { get; set; } = default!;
        public int QuantidadeDisponivel { get; set; }
        public decimal ValorReferencia { get; set; }
    }
}