

namespace MicroServicoEstoque.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string NomeProduto { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public decimal Preco { get; set; }
    }
}