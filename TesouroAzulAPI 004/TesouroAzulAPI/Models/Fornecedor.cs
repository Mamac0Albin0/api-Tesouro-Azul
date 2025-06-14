using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesouroAzulAPI.Models
{
    [Table("TB_FORNECEDOR")]
    public class Fornecedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_FORNECEDOR { get; set; }

        [Required, ForeignKey("Usuario")]
        public int ID_USUARIO_FK { get; set; }

        [Required, MaxLength(40)]
        public string NOME_FORNECEDOR { get; set; }

        [Required, StringLength(20)] // Olhar para depois para ser unico
        public string CNPJ_FORNECEDOR { get; set; }

        [Required, StringLength(35), EmailAddress]
        public string EMAIL_FORNECEDOR { get; set; }

        [StringLength(9)] // Olhar para depois para ser unico
        public string TEL_FORNECEDOR { get; set; }

        [Required, StringLength(15)]
        public string CEL_FORNECEDOR { get; set; }

        [Required, MaxLength(50)]
        public string ENDERECO_FORNECEDOR { get; set; }

        [ForeignKey(nameof(ID_USUARIO_FK))]
        public Usuario Usuario { get; set; } // FK para Usuario
    }
}
