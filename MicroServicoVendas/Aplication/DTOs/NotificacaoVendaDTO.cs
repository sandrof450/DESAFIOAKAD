namespace MicroServicoVendas.Aplication.DTOs
{
    public class NotificacaoVendaDTO
    {
        public int ProdutoId { get; set; }
        public int QuantidadeVendida { get; set; }
        public DateTime DataVenda { get; set; }
    }
}