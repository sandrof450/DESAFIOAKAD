using MicroServicoEstoque.Domain.Models;

namespace MicroServicoEstoque.Domain.Interfaces
{
    public interface IProdutoRepository
    {
        Task<Produto> CreateProdutoAsync(Produto produto);
        Task<List<Produto>> GetAllProdutosAsync();
        Task<Produto> GetProdutoPorIdAsync(int produtoId);
        Task<Produto> GetProdutoByNameAsync(string name);
        Task<Produto> UpdateprodutoAsync(int id, Produto produto);
    }
}