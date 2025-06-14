using Microsoft.AspNetCore.Mvc;
using TesouroAzulAPI.Data;

namespace TesouroAzulAPI.Controllers
{
    // Classe apenas para testar a conexão com o banco de dados e api
    [Route("api/TestarConexao"), ApiController]
    public class TestarConexaoController : ControllerBase
    {
        // Instandiando o context da ApplicationDbContext para _context
        private readonly ApplicationDbContext _context;

        public TestarConexaoController(ApplicationDbContext context) { _context = context; }

        // GETS
        // Testar Conexão
        [HttpGet("StatusBanco")]
        public async Task<IActionResult> TestarConexao()
        {
            // Verifica se a conexão com o banco de dados está funcionando
            try
            {
                if (await _context.Database.CanConnectAsync() == true) return Ok(new { mensagem = "Conexão com o banco de dados estabelecida com sucesso." });
                else return BadRequest("Banco de dados não encontrado");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao conectar ao banco de dados.", erro = ex.Message });
            }
        }

        [HttpGet("StatusAPI")]
        public IActionResult Status()
        {
            // Retorna um status de OK para indicar que a API está funcionando
            return Ok(new { mensagem = "API está funcionando corretamente." });
        }
    }
}
