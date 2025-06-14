using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesouroAzulAPI.Models
{
    [Table("TB_ESTOQUE_PRODUTO")]
    public class EstoqueProduto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_ESTOQUE { get; set; }

        [Required]
        public int ID_PRODUTO_FK { get; set; }
        
        [Required]
        public int ID_USUARIO_FK { get; set; }
        
        [Required, Column(TypeName="decimal(10,2)")]
        public decimal QTD_TOTAL_ESTOQUE { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal VALOR_GASTO_TOTAL_ESTOQUE { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal VALOR_POTENCIAL_VENDA_ESTOQUE { get; set; }

        public DateTime DATA_ATUALIZACAO_ESTOQUE { get; set; } = DateTime.Now;

        [ForeignKey(nameof(ID_PRODUTO_FK))]
        public Produto Produto { get; set; }

        [ForeignKey(nameof(ID_USUARIO_FK))]
        public Fornecedor Fornecedor { get; set; }
    }
}
