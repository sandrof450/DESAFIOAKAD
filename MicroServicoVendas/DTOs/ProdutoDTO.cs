using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoVendas.DTOs
{
    public class ProdutoDTO
    {
        public int Id { get; set; }
        public string NomeProduto { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public decimal Preco { get; set; }
    }
}