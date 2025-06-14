namespace TesouroAzulAPI.Dtos
{
    public class CriarFornecedorDto
    {
        public string NOME_FORNECEDOR { get; set; } = string.Empty;
        public string CNPJ_FORNECEDOR { get; set; } = string.Empty;
        public string EMAIL_FORNECEDOR { get; set; } = string.Empty;
        public string? TELEFONE_FORNECEDOR { get; set; } = string.Empty;
        public string CEL_FORNECEDOR { get; set; } = string.Empty;
        public string ENDERECO_FORNECEDOR { get; set; } = string.Empty;
    }
}
