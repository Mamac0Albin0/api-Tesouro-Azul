using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesouroAzulAPI.Models;
using TesouroAzulAPI.Data;


namespace TesouroAzulAPI.Controllers
{
    [Route("api/Metas"), ApiController]
    public class MetasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Instanciando com o contexto do banco de dados
        public MetasController(ApplicationDbContext context) { _context = context; }

        // DTOs

        // POSTs

        // GETs

        // PATCHs

        // DELETEs
    }
}
