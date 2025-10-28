using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroServicoVendas.Aplication.DTOs;

namespace MicroServicoVendas.Infrastructure.Clients.Interfaces
{
    public interface IEstoqueClient
    {
        Task<ProdutoDTO> GetProdutoAsync(int id);
        Task<ProdutoDTO> GetProdutoAsNameAsync(string name);
        Task<ProdutoDTO> UpdateProdutoAsync(int id, ProdutoDTO produtoDTO);
        void SetAuthorizationHeaderFromContext();
    }
}