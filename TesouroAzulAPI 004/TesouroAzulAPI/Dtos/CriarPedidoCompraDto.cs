using System.ComponentModel.DataAnnotations;

namespace TesouroAzulAPI.Dtos
{
    public class CriarPedidoCompraDto
    {
        public int? ID_FORNECEDOR { get; set; } 
        public decimal VALOR_VALOR { get; set; }

    }

    public class ItensCompraDto
    {
        public int ID_PRODUTO_FK { get; set; }
        public int ID_PEDIDO_FK { get; set; }
        public DateTime? VAL_ITEM_COMPRA { get; set; } = null;
        public string LOTE_COMPRA { get; set; }
        public decimal QUANTIDADE_ITEM_COMPRA { get; set; }
        public int N_ITEM_COMPRA { get; set; }
        public decimal VALOR_TOTAL_ITEM_COMPRA { get; set; }

    }
    public class PedidoCompraCompleto
    {
        public CriarPedidoCompraDto Pedido { get; set; }
        public List<ItensCompraDto>Item { get; set; }
    }
}
