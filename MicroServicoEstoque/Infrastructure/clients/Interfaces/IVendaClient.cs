using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServicoEstoque.Infrastructure.clients.Interfaces
{
    public interface IVendaClient
    {
        Task<List<int>> GetListCountVendasMensaisByProdutoAsync(int produtoId);
        void SetAuthorizationHeaderFromContext();
    }
}