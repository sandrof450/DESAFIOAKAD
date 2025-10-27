using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MicroServicoEstoque.IA
{
    public class IAService: IIAService
    {

        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly ILogger<IAService> _logger;

        public IAService(IConfiguration configuration, IHttpClientFactory httpFactory, ILogger<IAService> logger = null)
        {
            var _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API key is not configured.");
            _http = httpFactory.CreateClient(nameof(IAService));
            _logger = logger;

            _http.BaseAddress = new Uri("https://api.openai.com/");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> DemandaPrevistaAsync(
            string descricaoProduto,
            string categoriaProduto,
            int quantidadeEstoque,
            List<int> vendasMensais,
            int leadTimeDias
        )
        {
            var promptSystem = @"
            Você é um analista de estoque com foco em previsão de demanda.
            Analise o histórico de vendas, o estoque atual e o lead time para prever o comportamento futuro.

            ⚠️ INSTRUÇÃO IMPORTANTE:
            Retorne **APENAS** um JSON válido, sem explicações, comentários ou texto fora do objeto JSON.
            Não inclua frases como 'Portanto, o JSON seria' ou 'Segue o resultado'.
            Responda SOMENTE com o JSON.

            O JSON deve conter:
            {
            ""tendencia_vendas"": ""alta"" | ""queda leve"" | ""estável"",
            ""variacao_percentual_media"": número (pode ser negativo),
            ""projecao_vendas_proximos_30_dias"": número inteiro,
            ""previsao_esgotamento_dias"": número inteiro,
            ""nivel_risco"": ""baixo"" | ""médio"" | ""alto"",
            ""alerta"": ""<mensagem curta>"",
            ""acao_recomendada"": ""<mensagem prática>""
            }

            Use o raciocínio interno, mas **não o exponha na resposta final**.
            ";

            var vendasMensaisStr = vendasMensais is null ? "" : string.Join(", ", vendasMensais);
            var promptUsuario = $@"Dados do produto:
            - Descrição: {descricaoProduto}
            - Categoria: {categoriaProduto}
            - Quantidade atual em estoque: {quantidadeEstoque}
            - Vendas mensais: {vendasMensaisStr}
            - Tempo médio de reposição (lead time): {leadTimeDias} dias";

            

            var payload = new
            {
                model = "gpt-4",
                messages = new object[]
                {
                    new { role = "system", content = promptSystem },
                    new { role = "user", content = promptUsuario }
                },
                temperature = 0.0,         // more deterministic responses for precise JSON
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0,
                max_tokens = 250,
                n = 1
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await _http.PostAsync("v1/chat/completions", content);
            var respBody = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                _logger?.LogError("OpenAI error: {Status} - {Body}", resp.StatusCode, respBody);
                throw new HttpRequestException($"OpenAI API error: {resp.StatusCode}");
            }

            using var doc = JsonDocument.Parse(respBody);
            var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            return text.Trim();
        }

    }
}