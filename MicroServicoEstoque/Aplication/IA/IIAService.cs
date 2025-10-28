using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoEstoque.Aplication.IA
{
    public interface IIAService
    {
        Task<string> DemandaPrevistaAsync(
            string descricaoProduto,
            string categoriaProduto,
            int quantidadeEstoque,
            List<int> vendasMensais,
            int leadTimeDias
        );
    }
}