using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace TesouroAzulAPI.Dtos
{
    public class CadastrarProdutoDto
    {
        public string COD_PRODUTO { get; set; }
        [MaxLength(20), Required]
        public string NOME_PRODUTO { get; set; }
        public decimal VALOR_PRODUTO { get; set; }
        [MaxLength(40), Required]
        public string TIPO_PRODUTO { get; set; }
        public string? IMG_PRODUTO { get; set; } = null;

    }
}
