
using MicroServicoEstoque.Aplication.DTOs;
using MicroServicoEstoque.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicoEstoque.Aplication.Services.Interfaces
{
    public interface IProdutoService
    {
        Task<Produto> CreateProdutoAsync(Produto produto);
        Task<List<Produto>> GetAllProdutosAsync();
        Task<Produto> GetProdutoPorIdAsync(int produtoId);
        Task<Produto> GetProdutoByNameAsync(string name);
        Task<DemandaPrevistaDTO> GetDemandaPrevistaAsync(int produtoId);
        Task<Produto> UpdateProdutoAsync(int id, [FromBody] Produto produto);
        T DeserializeJson<T>(string jsonString);
    }
}