using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Models;

namespace TesouroAzulAPI.Controllers
{
    [Route("api/Vencimento")]
    public class VencimentoController : ControllerBase
    {
        public readonly ApplicationDbContext _context;

        // Instanciando o context do banco
        public VencimentoController(ApplicationDbContext context) { _context = context; }

        // Dtos

        // A lógica de vencimento sempre seguirá o seguinte padrão:
        // 1. Busca o item por usuario
        // 2. Busca o item pela tabela de Itens Compra
        // 3. Utiliza o Código como valor de identificação
        // 4. Verifica se o item está vencido com a data atual

        // POSTs
        // Gerar buscar vencimentos de itens por campo

        // GETs
        // Buscar vencimentos de itens de todos usuarios
        [Authorize(Roles = "admin")]
        [HttpGet("buscar-itens-vencidos-usuarios")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarVencidosUsuarios()
        {
            var ItensVencidos = await _context.ItensCompra
                .Where(i => i.VAL_ITEM_COMPRA < DateTime.Now && i.ESTADO_ITEM_COMPRA != "vendido")
                .ToListAsync();
            if (ItensVencidos == null || !ItensVencidos.Any()) return NotFound("Nenhum item vencido encontrado.");
            return Ok(ItensVencidos);
        }

        // Buscar vencimento de itens de todos usuarios do mês
        [Authorize(Roles = "admin")]
        [HttpGet("buscar-itens-vencidos-mes-usuarios")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarVencidosMesUsuarios()
        {
            var ItensVencidosMes = await _context.ItensCompra
                .Where(i => i.VAL_ITEM_COMPRA < DateTime.Now && i.ESTADO_ITEM_COMPRA != "vendido" && i.VAL_ITEM_COMPRA.Value.Month == DateTime.Now.Month)
                .ToListAsync();
            if (ItensVencidosMes == null || !ItensVencidosMes.Any()) return NotFound("Nenhum item vencido encontrado no mês atual.");
            return Ok(ItensVencidosMes);
        }

        // Buscar vencimentos de itens do usuario 
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-vencidos-usuario")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarVencidosUsuario()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Pela tabela ItensCompra não possuir o ID_USUARIO_FK, é necessário buscar os itens do usuario através do PedidoCompra
            var ItensVencidosUsuario = await _context.ItensCompra
                .Where(i => i.VAL_ITEM_COMPRA < DateTime.Now && i.ESTADO_ITEM_COMPRA != "vendido" &&
                            _context.PedidosCompra.Any(p => p.ID_USUARIO_FK == IdUsuario && p.ID_PEDIDO == i.ID_PEDIDO_FK))
                .ToListAsync();

            if (ItensVencidosUsuario == null || !ItensVencidosUsuario.Any()) return NotFound("Nenhum item vencido encontrado para o usuário atual.");
            return Ok(ItensVencidosUsuario);
        }

        // Buscar vencimento de item do usuario
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-vencidos-usuario/{id_produto}")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarVencidosUsuario(int id_produto)
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // Pela tabela ItensCompra não possuir o ID_USUARIO_FK, é necessário buscar os itens do usuario através do PedidoCompra
            var ItensVencidosUsuario = await _context.ItensCompra
                .Where(i => i.VAL_ITEM_COMPRA < DateTime.Now && i.ESTADO_ITEM_COMPRA != "vendido" &&
                            i.ID_PRODUTO_FK == id_produto &&
                            _context.PedidosCompra.Any(p => p.ID_USUARIO_FK == IdUsuario && p.ID_PEDIDO == i.ID_PEDIDO_FK))
                .ToListAsync();
            if (ItensVencidosUsuario == null || !ItensVencidosUsuario.Any()) return NotFound("Nenhum item vencido encontrado para o usuário atual.");
            return Ok(ItensVencidosUsuario);
        }

        // Buscar vencimentos de itens do usuario do mês  
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-vencidos-mes-usuario")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarVencidosMesUsuario()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // Pela tabela ItensCompra não possuir o ID_USUARIO_FK, é necessário buscar os itens do usuario através do PedidoCompra  
            var ItensVencidosMesUsuario = await _context.ItensCompra
                .Where(i => i.VAL_ITEM_COMPRA.HasValue &&
                            i.VAL_ITEM_COMPRA.Value < DateTime.Now &&
                            i.ESTADO_ITEM_COMPRA != "vendido" &&
                            i.VAL_ITEM_COMPRA.Value.Month == DateTime.Now.Month &&
                            _context.PedidosCompra.Any(p => p.ID_USUARIO_FK == IdUsuario && p.ID_PEDIDO == i.ID_PEDIDO_FK))
                .ToListAsync();
            if (ItensVencidosMesUsuario == null || !ItensVencidosMesUsuario.Any()) return NotFound("Nenhum item vencido encontrado no mês atual para o usuário.");
            return Ok(ItensVencidosMesUsuario);
        }
        // Buscar vencimentos de itens do usuario do ano
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-vencidos-ano-usuario")]
        public async Task<ActionResult<IEnumerable<ItensLucro>>> BuscarVencidosAnoUsuario()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // Pela tabela ItensCompra não possuir o ID_USUARIO_FK, é necessário buscar os itens do usuario através do PedidoCompra  
            var ItensVencidosAnoUsuario = await _context.ItensCompra
                .Where(i => i.VAL_ITEM_COMPRA.HasValue &&
                            i.VAL_ITEM_COMPRA.Value < DateTime.Now &&
                            i.ESTADO_ITEM_COMPRA != "vendido" &&
                            i.VAL_ITEM_COMPRA.Value.Year == DateTime.Now.Year &&
                            _context.PedidosCompra.Any(p => p.ID_USUARIO_FK == IdUsuario && p.ID_PEDIDO == i.ID_PEDIDO_FK))
                .ToListAsync();
            if (ItensVencidosAnoUsuario == null || !ItensVencidosAnoUsuario.Any()) return NotFound("Nenhum item vencido encontrado no ano atual para o usuário.");
            return Ok(ItensVencidosAnoUsuario);
        }
    }
}
