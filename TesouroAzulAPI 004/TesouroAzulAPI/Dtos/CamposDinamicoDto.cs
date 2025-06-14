using System.ComponentModel.DataAnnotations;

namespace TesouroAzulAPI.Dtos
{
    public class CamposDinamicoDto
    {
        [Required]
        public string Campo { get; set; } = string.Empty;
        [Required]
        public string Valor { get; set; } = string.Empty;

    }
}
