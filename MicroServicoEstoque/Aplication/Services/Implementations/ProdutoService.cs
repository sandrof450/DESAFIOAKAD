using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MicroServicoEstoque.Aplication.DTOs;
using MicroServicoEstoque.Aplication.IA;
using MicroServicoEstoque.Aplication.Services.Interfaces;
using MicroServicoEstoque.Domain.Interfaces;
using MicroServicoEstoque.Domain.Models;
using MicroServicoEstoque.Infrastructure.clients.Interfaces;
using MicroServicoEstoque.Infrastructure.Repositories.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicoEstoque.Aplication.Services.Implementations
{
    public class ProdutoService: IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IIAService _iaService;
        private readonly IVendaClient _vendaClient;

        public ProdutoService(IProdutoRepository produtoRepository, IIAService iaService, IVendaClient vendaClient)
        {
            _produtoRepository = produtoRepository;
            _iaService = iaService;
            _vendaClient = vendaClient;                      
        }

        public async Task<Produto> CreateProdutoAsync(Produto produto)
        {
            #region Validate produto is not null
            if (produto == null)
                throw new ArgumentNullException(nameof(produto), "produto is not can null");
            #endregion

            #region Validate if product already exists
            var ExistsProductByName = await _produtoRepository.GetProdutoByNameAsync(produto.NomeProduto) != null;

            if (ExistsProductByName == true)
                throw new Exception($"Product by name '{produto.NomeProduto}' already exists.");
            #endregion

            #region Add the new produto to the context
            var novoProduto = await _produtoRepository.CreateProdutoAsync(produto);
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

        public async Task<Produto> GetProdutoPorIdAsync(int produtoId)
        {
            #region Get Produto to the context
            var produto = await _produtoRepository.GetProdutoPorIdAsync(produtoId);
            #endregion

            #region Validate produto is not null
            if (produto == null)
            {
                throw new Exception($"Produto with id {produtoId} not found.");
            }
            #endregion

            return produto;
        }

        public async Task<Produto> GetProdutoByNameAsync(string name)
        {
            #region Validate produto is null or empty
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception($"Produto with name '{name}' not found.");
            }
            #endregion

            #region Get Produto to the context
            var produto = await _produtoRepository.GetProdutoByNameAsync(name);
            #endregion


            return produto;
        }

        public async Task<DemandaPrevistaDTO> GetDemandaPrevistaAsync(int produtoId)
        {
            #region Get Produto to the context
            var produto = await GetProdutoPorIdAsync(produtoId);
            #endregion

            #region Pega os pedidos dos dias de acordo com o que vier no parametro, por exemplo 12 dias
            var vendasMensais = await _vendaClient.GetListCountVendasMensaisByProdutoAsync(produtoId);
            #endregion 

            #region Demanda prevista logic
            var demandaPrevista = await _iaService.DemandaPrevistaAsync
            (
                descricaoProduto: produto.NomeProduto,
                categoriaProduto: "Categoria teste",
                quantidadeEstoque: produto.QuantidadeDisponivel,
                vendasMensais: vendasMensais,
                leadTimeDias: 7
            );
            #endregion

            #region validate demanda prevista is not null
            if (string.IsNullOrEmpty(demandaPrevista))
            {
                throw new Exception("Failed to get demanda prevista from IA service.");
            }
            #endregion

            var demandaPrevistaJsonLimpo = DeserializeJson<DemandaPrevistaDTO>(demandaPrevista);

            var demandaPrevistaDTO = new DemandaPrevistaDTO()
            {
                variacao_percentual_media = demandaPrevistaJsonLimpo.variacao_percentual_media,
                tendencia_vendas = demandaPrevistaJsonLimpo.tendencia_vendas,
                projecao_vendas_proximos_30_dias = demandaPrevistaJsonLimpo.projecao_vendas_proximos_30_dias,
                previsao_esgotamento_dias = demandaPrevistaJsonLimpo.previsao_esgotamento_dias,
                nivel_risco = demandaPrevistaJsonLimpo.nivel_risco,
                alerta = demandaPrevistaJsonLimpo.alerta,
                acao_recomendada = demandaPrevistaJsonLimpo.acao_recomendada,
            };

            return demandaPrevistaDTO;
        }

        public async Task<Produto> UpdateProdutoAsync(int id, [FromBody] Produto produto)
        {
            #region Validate produto is not null or Not exists
            var existsProduto = await _produtoRepository.GetProdutoPorIdAsync(id) != null;
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
            var produtoUpdate = await _produtoRepository.UpdateprodutoAsync(id, produto);
            #endregion

            return produtoUpdate;
        }

        public T DeserializeJson<T>(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                throw new ArgumentException("JSON de entrada está vazio.");

            // 1️⃣ Limpeza básica (sem remover todas as barras!)
            var jsonRaw = jsonString.Trim();

            // 2️⃣ Tenta extrair o conteúdo entre { e } se vier com ruído ao redor
            var match = Regex.Match(jsonRaw, "\\{.*\\}", RegexOptions.Singleline);
            if (match.Success)
                jsonRaw = match.Value;

            // 3️⃣ Se o JSON estiver com aspas e conteúdo escapado (ex: "{\"chave\":\"valor\"}")
            //     — então precisamos desserializar primeiro como string
            if (jsonRaw.StartsWith("\"") && jsonRaw.EndsWith("\""))
            {
                jsonRaw = JsonSerializer.Deserialize<string>(jsonRaw)!;
            }

            // 4️⃣ Agora sim, faz a desserialização real
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            return JsonSerializer.Deserialize<T>(jsonRaw, options)
                ?? throw new JsonException("Falha ao desserializar o JSON.");
        }
    }
}

namespace MicroServicoEstoque.Aplication.DTOs
{
    public class DemandaPrevistaDTO
    {
        public double variacao_percentual_media { get; set; }
        public string tendencia_vendas { get; set; } = string.Empty;
        public int projecao_vendas_proximos_30_dias { get; set; }
        public int previsao_esgotamento_dias { get; set; }
        public string nivel_risco { get; set; } = string.Empty;
        public string alerta { get; set; } = string.Empty;
        public string acao_recomendada { get; set; } = string.Empty;
    }
}