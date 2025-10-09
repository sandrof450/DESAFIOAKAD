using MicroServicoEstoque.Models;
using MicroServicoEstoque.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MicroServicoEstoque.Repositories
{
    public class ProdutoRepository
    {
        private readonly EstoqueContext _context;

        public ProdutoRepository(EstoqueContext context)
        {
            _context = context;
        }

        public async Task<Produto> CreateProduto(Produto produto)
        {
            var novoProduto = await _context.Produtos.AddAsync(produto);

            var validateIfSaveChanges = _context.SaveChanges() > 0;

            if (!validateIfSaveChanges)
            {
                throw new Exception("Failed to save produto.");
            }

            return novoProduto.Entity;
        }

        public Produto GetProdutoPorId(int id)
        {
            var produto = _context.Produtos.AsNoTracking().FirstOrDefault(p => p.Id == id);

            return produto;
        }

        public async Task<Produto> GetProdutoAsName(string name)
        {
            var produto = await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.NomeProduto == name);

            return produto;
        }

        public Produto Updateproduto(int id, Produto produto)
        {
            var produtoUpdate = _context.Produtos.AsNoTracking().FirstOrDefault(p => p.Id == id);
            produtoUpdate.NomeProduto = produto.NomeProduto;
            produtoUpdate.Preco = produto.Preco;
            produtoUpdate.QuantidadeDisponivel = produto.QuantidadeDisponivel;

            _context.Produtos.Update(produtoUpdate);

            var validateIfSaveChanges = _context.SaveChanges() > 0;

            if (!validateIfSaveChanges)
            {
                throw new Exception("Failed to save produto.");
            }

            return produtoUpdate;
        }

    }
}