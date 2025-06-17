using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.VisualBasic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Numerics;

namespace TesouroAzulAPI.Models
{
    [Table("TB_USUARIO")]
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID_USUARIO { get; set; }

        [Required, StringLength(80)]
        public string NOME_USUARIO { get; set; }

        [Required, MaxLength(35), EmailAddress]
        public string EMAIL_USUARIO { get; set; }

        [Required]
        public DateTime DATA_NASC_USUARIO { get; set; }

        [StringLength(11)]
        public string? CPF_USUARIO {  get; set; }

        [StringLength(14)]
        public string? CNPJ_USUARIO { get; set; }

        [Required]
        public int ID_ASSINATURA_FK { get; set; } = 0; // 1 para não assinante; 2 para assinante

        [Column(TypeName = "MEDIUMBLOB")]
        public byte[]? FOTO_USUARIO { get; set; }

        [Required, MinLength(8), MaxLength(20)]
        public string SENHA_USUARIO { get; set; }       

        [Required, StringLength(1)]
        public string STATUS_USUARIO { get; set; } = "a"; // a para ativo; d para inativo

        [Required]
        public DateTime DATA_INICIO_ASSINATURA_USUARIO { get; set; } = DateTime.Now;

        [Required]
        public DateTime DATA_VALIDADE_ASSINATURA_USUARIO { get; set; }

        [ForeignKey(nameof(ID_ASSINATURA_FK))]
        public Assinatura Assinatura { get; set; } // FK para Assinatura
    }
}
