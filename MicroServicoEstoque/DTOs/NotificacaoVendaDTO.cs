using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoEstoque.DTOs
{
    public class NotificacaoVendaDTO
    {
        public int ProdutoId { get; set; }
        public int QuantidadeVendida { get; set; }
        public DateTime DataVenda { get; set; }
    }
}