using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoEstoque.Aplication.DTOs
{
    public class ProdutoDTO
    {
        public int Id { get; set; }
        public string NomeProduto { get; set; } = default!;
        public int QuantidadeDisponivel { get; set; }
        public decimal Preco { get; set; }
    }
}