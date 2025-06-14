using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesouroAzulAPI.Models
{
    [Table("TB_PRODUTO")]
    public class Produto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_PRODUTO { get; set; }

        [Required]
        public int ID_USUARIO_FK { get; set; }

        [Required, MaxLength(80)]
        public string COD_PRODUTO { get; set; }

        [Required, MaxLength(20)]
        public string NOME_PRODUTO { get; set;}

        [Required, Column(TypeName = "decimal(8,2)")]
        public decimal VALOR_PRODUTO { get; set;}

        [Required, MaxLength(40)]
        public string TIPO_PRODUTO { get; set;}

        [Column(TypeName = "MEDIUMBLOB")]
        public byte[]? IMG_PRODUTO { get; set;}

        [ForeignKey(nameof(ID_USUARIO_FK))]
        public Fornecedor Fornecedor { get; set;}
    }
}
