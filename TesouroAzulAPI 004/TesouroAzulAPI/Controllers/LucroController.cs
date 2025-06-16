using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Dtos;
using TesouroAzulAPI.Models;

namespace TesouroAzulAPI.Controllers
{
    [Route("api/Lucro")]
    public class LucroController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Instanciando o context do banco
        public LucroController(ApplicationDbContext context) { _context = context; }

        // Devido à limitação de estrutura do banco de dados, pouco será possivel desenvolver dentro do back end com o tempo atual utilizando apenas conceitos simples sem
        // desenvolver muito 


        // POSTs
        // Criar lucro
        [Authorize(Roles = "Administrador, Usuario")]
        [HttpPost("criar-lucro")]
        public async Task<IActionResult> CriarLucroCompleto([FromBody] LucroCompletoDto dto)
        {
            int valorTotal;
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Cria o lucro
            var lucro = new Lucro
            {
                VALOR_LUCRO = 0,
                ID_USUARIO_FK = IdUsuario,
                DATA_CONTABILIZACAO_LUCRO = DateTime.Now
            };

            _context.Lucros.Add(lucro);
            await _context.SaveChangesAsync();


            return default(IActionResult); // Implementar a lógica de criação do lucro completo
        }

        // GETs
        // Buscar lucros de todos usuarios

        // Buscar itens lucro de todos usuarios

        // Buscar lucro total do usuario {id}
        [Authorize(Roles = "user")]
        [HttpGet("buscar-lucro-total-usuario")]
        public async Task<ActionResult<decimal>> BuscarLucroTotalUsuario()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var totalLucro = await _context.Lucros
                .Where(l => l.ID_USUARIO_FK == IdUsuario)
                .SumAsync(l => l.VALOR_LUCRO);
            if (totalLucro == 0) return NotFound("Nenhum lucro encontrado para o usuário.");
            return Ok(totalLucro);
        }

        // Buscar lucro total de item do usuario {id}
        [Authorize(Roles = "user")]
        [HttpGet("buscar-lucro-total-item-usuario")]
        public async Task<ActionResult<decimal>> BuscarLucroTotalItemUsuario(int idItem)
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Verifica se o item existe
            var item = await _context.Produtos.FindAsync(idItem);
            if (item == null) return NotFound("Item não encontrado.");

            // Pega o VALOR_TOTAL_ITEM_VENDA atravez do id
            decimal valorTotalVenda = await _context.ItensVenda
                .Where(i => i.ID_PRODUTO_FK == idItem && i.PedidoVenda.ID_USUARIO_FK == IdUsuario)
                .SumAsync(i => i.VALOR_TOTAL_ITEM_VENDA);

            // Pega o VALOR_TOTAL_ITEM_COMPRA atravez do id
            decimal valorTotalCompra = await _context.ItensCompra
                .Where(i => i.ID_PRODUTO_FK == idItem && i.PedidoCompra.ID_USUARIO_FK == IdUsuario)
                .SumAsync(i => i.VALOR_TOTAL_ITEM_COMPRA);

            // Calcula o lucro total do item

            decimal lucroTotalItem = valorTotalVenda - valorTotalCompra;

            return Ok(lucroTotalItem);


        }

        // Buscar lucro de um usuario {id} por data
        [Authorize(Roles = "user")]
        [HttpGet("buscar-lucro-total-por-data")]
        public async Task<ActionResult<decimal>> BuscarLucroTotalPorData(DateTime data)
        {
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return default(ActionResult<decimal>); // Implementar a lógica de busca do lucro total por data

        }


        // Buscar lucro de um usuario {id} por data e item


        // DELETEs
        // Deletar Lucro


    }
}
