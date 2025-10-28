using System.Net.Http.Headers;
using System.Text.Json;
using MicroServicoEstoque.Infrastructure.clients.Interfaces;

namespace MicroServicoEstoque.Infrastructure.clients.Implementations
{
    public class VendaClient: IVendaClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string? _token = "";

        public VendaClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            //**Insere o token JWT do contexto HTTP atual no cabeçalho Authorization do HttpClient, isso é necessário para autenticação em chamadas de API**//
            SetAuthorizationHeaderFromContext();
        }

        public async Task<List<int>> GetListCountVendasMensaisByProdutoAsync(int produtoId)
        {
            var response = await _httpClient.GetAsync($"/api/Pedido/GetListCountVendasMensaisByProduto?produtoId={produtoId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<int>();
            }

            var listCountVendaMensaisConsumer = JsonSerializer.Deserialize<List<int>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return listCountVendaMensaisConsumer ?? new List<int>();
        }
        
        public void SetAuthorizationHeaderFromContext()
        {
            //**Insere o token JWT do contexto HTTP atual no cabeçalho Authorization do HttpClient, isso é necessário para autenticação em chamadas de API**//
            // _token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"];

            //Necessário inserir manualmente o token quando utilizar o projeto localmente
            var _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJFbWFpbCI6InRlc3RlQGdtYWlsLmNvbSIsIlBlcmZpbCI6IkFkbWluIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3NjE2NjgxOTh9.CZ9oms3hh9fNRO8J7Q2bF3RCL54IWPurZw_xguq-NNg";
            if (string.IsNullOrEmpty(_token))
            {
                throw new Exception("Authorization token is missing in the HTTP context.");
            }
            // Set the Authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token.Replace("Bearer ", ""));
        }
        
    }
}