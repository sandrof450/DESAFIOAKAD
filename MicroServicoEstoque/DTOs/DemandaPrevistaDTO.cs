namespace MicroServicoEstoque.DTOs
{
    public class DemandaPrevistaDTO
    {
        public string tendencia_vendas { get; set; } = default!;
        public decimal variacao_percentual_media { get; set; }
        public decimal projecao_vendas_proximos_30_dias { get; set; }
        public decimal previsao_esgotamento_dias { get; set; }
        public string nivel_risco { get; set; } = default!;
        public string alerta { get; set; } = default!;
        public string acao_recomendada { get; set; } = default!;
    }
}