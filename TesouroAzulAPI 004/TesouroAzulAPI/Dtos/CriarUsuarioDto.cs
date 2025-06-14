using System.ComponentModel.DataAnnotations;

namespace TesouroAzulAPI.Dtos
{
    public class CriarUsuarioDto
    {
        public string NOME_USUARIO { get; set; } = string.Empty;
        [MaxLength(35), EmailAddress]
        public string EMAIL_USUARIO { get; set; } = string.Empty;
        public DateTime DATA_NASC_USUARIO { get; set; }
        public string CPF_USUARIO { get; set; } = string.Empty;
        public string? CNPJ_USUARIO { get; set; } = string.Empty;
        public int ID_ASSINATURA_FK { get; set; } = 0; // 1 para não assinante; 2 para assinante
        public string? FOTO_USUARIO { get; set; } = null;
        public string SENHA_USUARIO { get; set; } = string.Empty;


    }
}
