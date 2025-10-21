using MicroServicoEstoque.Models;
using MicroServicoEstoque.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicoEstoque.Service
{
    public class ProdutoService
    {
        private readonly ProdutoRepository _produtoRepository;

        public ProdutoService(ProdutoRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public async Task<Produto> CreateProduto(Produto produto)
        {
            #region Validate produto is not null
            if (produto == null)
                throw new ArgumentNullException(nameof(produto), "produto is not can null");
            #endregion

            #region Validate if product already exists
            var ExistsProductByName = await _produtoRepository.GetProdutoAsName(produto.NomeProduto) != null;

            if (ExistsProductByName == true)
                throw new Exception($"Product by name '{produto.NomeProduto}' already exists.");
            #endregion

            #region Add the new produto to the context
            var novoProduto = await _produtoRepository.CreateProduto(produto);
            #endregion

            return novoProduto;
        }

        public async Task<List<Produto>> GetAllProdutosAsync()
        {
            #region Get Produto to the context
            var produtos = await _produtoRepository.GetAllProdutosAsync();
            #endregion

            #region Validate produto is not null
            if (produtos.Count == 0)
            {
                throw new Exception($"No produtos found.");
            }
            #endregion

            return produtos;
        }

        public Produto GetProdutoPorId(int id)
        {
            #region Get Produto to the context
            var produto = _produtoRepository.GetProdutoPorId(id);
            #endregion

            #region Validate produto is not null
            if (produto == null)
            {
                throw new Exception($"Produto with id {id} not found.");
            }
            #endregion

            return produto;
        }

        public async Task<Produto> GetProdutoAsName(string name)
        {
            #region Validate produto is null or empty
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception($"Produto with name '{name}' not found.");
            }
            #endregion

            #region Get Produto to the context
            var produto = await _produtoRepository.GetProdutoAsName(name);
            #endregion


            return produto;
        }

        public Produto UpdateProduto(int id, [FromBody] Produto produto)
        {
            #region Validate produto is not null or Not exists
            var existsProduto = _produtoRepository.GetProdutoPorId(id) != null;
            if (!existsProduto)
            {
                throw new Exception("produto not found");
            }

            if (produto == null)
            {
                throw new Exception($"produto with id {id} not found.");
            }
            #endregion

            #region Update produto to the context
            var produtoUpdate = _produtoRepository.Updateproduto(id, produto);
            #endregion

            return produtoUpdate;
        }
    }
}