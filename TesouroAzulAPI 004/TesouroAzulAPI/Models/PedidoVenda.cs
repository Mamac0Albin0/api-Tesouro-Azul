using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TesouroAzulAPI.Models
{
    [Table("TB_PEDIDO_VENDA")]
    public class PedidosVenda
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_PEDIDO_VENDA { get; set; }

        [Required]
        public int ID_USUARIO_FK { get; set; }

        [Required]
        public DateTime DATA_PEDIDO_VENDA { get; set; } = DateTime.Now;

        [Required, Column(TypeName = "decimal(7,2)")]
        public decimal VALOR_PEDIDO_VENDA { get; set; } // Calculado dentro do DB

        [ForeignKey(nameof(ID_USUARIO_FK))]
        public Usuario Usuario { get; set; }
    }
    [Table("TB_ITEM_VENDA")]
    public class ItensVenda
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_ITEM_VENDA { get; set; }
        
        [Required]
        public int ID_PRODUTO_FK { get; set; }
        
        [Required]
        public int ID_PEDIDO_VENDA_FK { get; set; }

        /*
           Interessante para o futuro, no momento devido à falta de tempo não será aplicado
           public DateTime? VAL_ITEM_VENDA { get; set; } = null;

        */

        [MaxLength(255)]
        public string? LOTE_VENDA { get; set; }
        
        [Required, Column(TypeName = "decimal(8,2)")]
        public decimal QTS_ITEM_VENDA { get; set; }
        
        [Required]
        public int N_ITEM_VENDA { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DESCONTO_ITEM_VENDA { get; set; } = 0.00m; // Sempre utilizar valores absolutos/inteiros

        [Column(TypeName = "decimal(10,2)")]
        public decimal VALOR_TOTAL_ITEM_VENDA { get; set; }
        
        [ForeignKey(nameof(ID_PRODUTO_FK))]
        public Produto Produto { get; set; }
        
        [ForeignKey(nameof(ID_PEDIDO_VENDA_FK))]
        public PedidosVenda PedidoVenda { get; set; }
    }
}
