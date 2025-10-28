using MicroServicoEstoque.Domain.Interfaces;
using MicroServicoEstoque.Domain.Models;
using MicroServicoEstoque.Infrastructure.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace MicroServicoEstoque.Infrastructure.Repositories.Implementations
{
    public class ProdutoRepository: IProdutoRepository
    {
        private readonly EstoqueContext _context;

        public ProdutoRepository(EstoqueContext context)
        {
            _context = context;
        }

        public async Task<Produto> CreateProdutoAsync(Produto produto)
        {
            var novoProduto = await _context.Set<Produto>().AddAsync(produto);

            var validateIfSaveChanges = (await _context.SaveChangesAsync()) > 0;

            if (!validateIfSaveChanges)
            {
                throw new Exception("Failed to save produto.");
            }

            return novoProduto.Entity;
        }

        public async Task<List<Produto>> GetAllProdutosAsync()
        {
            var produtos = await _context.Produtos.AsNoTracking().ToListAsync();

            return produtos;
        }

        public async Task<Produto> GetProdutoPorIdAsync(int produtoId)
        {
            var produto = await _context.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == produtoId);

            return produto;
        }

        public async Task<Produto> GetProdutoByNameAsync(string name)
        {
            var produto = await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.NomeProduto == name);

            return produto;
        }

        public async Task<Produto> UpdateprodutoAsync(int id, Produto produto)
        {
            var produtoUpdate = await _context.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            produtoUpdate.NomeProduto = produto.NomeProduto;
            produtoUpdate.ValorReferencia = produto.ValorReferencia;
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