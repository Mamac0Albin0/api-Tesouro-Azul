using Microsoft.AspNetCore.Mvc;

namespace TesouroAzulAPI.Dtos
{
    public class CriarEstoqueDto
    {
        public int ID_PRODUTO_FK { get; set; }
        public decimal QTD_TOTAL_ESTOQUE { get; set; }
        public decimal VALOR_GASTO_TOTAL_ESTOQUE { get; set; }
        public decimal VALOR_POTENCIAL_VENDA_ESTOQUE { get; set; }

    }
}
