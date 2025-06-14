using Microsoft.AspNetCore.Mvc;

namespace TesouroAzulAPI.Dtos
{
    public class CriarPedidoVendaDto
    {
        public decimal? VALOR_PEDIDO_VENDA { get; set; } = null; // Calculado dentro do DB

    }
    public class ItensVendaDto
    {
        public int ID_PRODUTO_FK { get; set; }
        public string? LOTE_VENDA { get; set; }
        public decimal QTS_ITEM_VENDA { get; set; }
        public int N_ITEM_VENDA { get; set; }
        public decimal? DESCONTO_ITEM_VENDA { get; set; } = 0.00m;
        public decimal VALOR_TOTAL_ITEM_VENDA { get; set; }
    }

    public class PedidoVendaCompleto
    {
        public CriarPedidoVendaDto Pedido { get; set; }
        public List<ItensVendaDto> Item { get; set; }
    }
}