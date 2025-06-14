using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Dtos;

namespace TesouroAzulAPI.Controllers
{
    [Route("api/Lucro")]
    public class LucroController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Instanciando o context do banco
        public LucroController(ApplicationDbContext context) { _context = context; }

        // POSTs
        // Criar lucro
        [Authorize(Roles = "Administrador, Usuario")]
        [HttpPost("criar-lucro")]
        public async Task<IActionResult> CriarLucroCompleto([FromBody] LucroCompletoDto dto)
        {
            return default(IActionResult); // Implementar a lógica de criação do lucro completo
        }

        // Criar ItensLucro

        // Buscar lucro por campo

        // Buscar ItensLucro por campo 

        // GETs
        // Buscar lucros de todos usuarios

        // Buscar itens lucro de todos usuarios

        // Buscar lucros do usuario {id}

        // Buscar itens lucro do usuario {id}

        // PATCHs
        // Alterar valor lucro


        // DELETEs
        // Deletar Lucro

        // Deletar Itens lucro

    }
}
