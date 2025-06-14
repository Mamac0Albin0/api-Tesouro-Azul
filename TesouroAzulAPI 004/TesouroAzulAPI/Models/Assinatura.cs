using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesouroAzulAPI.Models
{
    [Table("TB_ASSINATURA")]
    public class Assinatura
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_ASSINATURA { get; set; }

        [Required, StringLength(50)]
        public string DESC_ASSINATURA { get; set; }

        [Required, Range(0, 9999.99)]
        public double VALOR_ASSINATURA { get; set; }

        [Required]
        public int TIPO_ASSINATURA_FK { get; set; } // 1 para normal; 2 para assinante

        [Required]
        public int DURACAO_SEGUNDOS_ASSINATURA { get; set; } // Duração em segundos

        [ForeignKey(nameof(TIPO_ASSINATURA_FK))]
        public Tipo_Assinatura Tipo_Assinatura { get; set; } // FK para Tipo_Assinatura
    }

    [Table("TB_TIPO_ASSINATURA")]
    public class Tipo_Assinatura
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_TIPO { get; set; }

        [Required, StringLength(20)]
        public string DESC_TIPO { get; set; }
    }
}
