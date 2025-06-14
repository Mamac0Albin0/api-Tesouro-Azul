namespace TesouroAzulAPI.Dtos
{
    public class CriarLucroDto
    {
        public decimal VALOR_LUCRO { get; set; } // Valor do lucro a ser registrado
                
    }
    public class ItensLucroDto
    {


    }

    public class LucroCompletoDto
    {
        public CriarLucroDto Lucro { get; set; }
        public List<ItensLucroDto> Itens { get; set; }
    }
}
