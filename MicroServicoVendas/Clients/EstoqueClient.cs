using System.Net;
using System.Net.Http.Headers;
using MicroServicoVendas.DTOs;

namespace MicroServicoVendas.Clients
{
    public class EstoqueClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? token = "";

        public EstoqueClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            
            //**Insere o token JWT do contexto HTTP atual no cabeçalho Authorization do HttpClient, isso é necessário para autenticação em chamadas de API**//
            SetAuthorizationHeaderFromContext();
        }

        // public async Task<ProdutoDTO> CreateProdutoAsync(ProdutoDTO produto)
        // {
        //     var response = await _httpClient.PostAsJsonAsync("/api/produto", produto);
        //     response.EnsureSuccessStatusCode();

        //     var produtoDTO = await response.Content.ReadFromJsonAsync<ProdutoDTO>();
        //     return produtoDTO!;

        // }

        public async Task<ProdutoDTO> GetProdutoAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/Produto/{id}");
            response.EnsureSuccessStatusCode();

            var produtoDTO = await response.Content.ReadFromJsonAsync<ProdutoDTO>();
            return produtoDTO!;
        }

        public async Task<ProdutoDTO> GetProdutoAsNameAsync(string name)
        {
            var response = await _httpClient.GetAsync($"/api/Produto/GetProdutoAsName/{name}");

            if (!response.IsSuccessStatusCode)
                return null!;

            if (response.StatusCode == HttpStatusCode.NoContent)
                return null!;           

            var produtoDTO = await response.Content.ReadFromJsonAsync<ProdutoDTO>();
            return produtoDTO!;
        }

        public async Task<ProdutoDTO> UpdateProdutoAsync(int id, ProdutoDTO produtoDTO)
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/Produto/{id}", produtoDTO);
            response.EnsureSuccessStatusCode();

            var produtoUpdate = await response.Content.ReadFromJsonAsync<ProdutoDTO>();

            return produtoUpdate;

        }

        public void SetAuthorizationHeaderFromContext()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            //var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJFbWFpbCI6InRlc3RlQGdtYWlsLmNvbSIsIlBlcmZpbCI6IkFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3NjE2NjgxOTh9.CZ9oms3hh9fNRO8J7Q2bF3RCL54IWPurZw_xguq-NNg";
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception($"Token {token} is valid");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
        }
    }
}