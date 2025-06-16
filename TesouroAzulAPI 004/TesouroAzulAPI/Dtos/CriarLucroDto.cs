namespace TesouroAzulAPI.Dtos
{
    public class CriarLucroDto
    {
        public decimal VALOR_LUCRO { get; set; } // Valor do lucro a ser registrado
                
    }
    public class ItensLucroDto
    {
        public int ID_ITEM { get; set; } // ID do item de lucro

    }

    public class LucroCompletoDto
    {
        public List<ItensLucroDto> Itens { get; set; }
    }
}
