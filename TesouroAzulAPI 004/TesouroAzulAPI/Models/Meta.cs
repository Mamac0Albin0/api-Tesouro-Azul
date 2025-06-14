using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesouroAzulAPI.Models
{
    [Table("TB_META")]
    public class Meta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_META { get; set; }
        [Required]
        public int ID_USUARIO_FK { get; set; }

        [Required, Column(TypeName = "decimal(12,2)")]
        public decimal VALOR_DESEJADO_META { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal VALOR_ATINGIDO_META { get; set; }

        [Column(TypeName ="date")]
        public DateTime DATA_META { get; set; }

        public string STATUS_META { get; set; } // Sempre utilizar os campos "em_andamento" "concluida" "cancelada"

        [ForeignKey(nameof(ID_USUARIO_FK))]
        public Usuario Usuario { get; set; }
    }

    // Para produtos individuais
    [Table("TB_ITEM_META")]
    public class ItensMeta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_ITEM_META { get; set; }
        [Required]
        public int ID_USUARIO_FK { get; set; }
        [Required]
        public int ID_PRODUTO_FK { get; set; }
        [Required, Column(TypeName ="decimal(12,2)")]
        public decimal VALOR_DESEJADO_ITEM_META { get; set; }
        [Column(TypeName = "decimal(12,2)")]
        public decimal VALOR_ATINGIDO_ITEM_META { get; set; }
        [Column(TypeName = "decimal(8,2")]
        public decimal QNT_VENDIDA_META { get; set; }
        [Column(TypeName = "date")]
        public DateTime DATA_META_ITEM { get; set; }
        [Column(TypeName = "enum('em_andamento', 'concluida', 'cancelada')")]
        public string STATUS_META { get; set; } = "em_andamento"; // Sempre utilizar os campos "em_andamento" "concluida" "cancelada"
        [ForeignKey(nameof(ID_USUARIO_FK))]
        public Usuario Usuario { get; set; }
        [ForeignKey(nameof(ID_PRODUTO_FK))]
        public Produto Produto { get; set; }
    }

}
