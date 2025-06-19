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
        // 5. Verifica se o item não foi vendido com a tabela ItensVenda

        // POSTs
        // Gerar buscar vencimentos de itens por campo

        // GETs
        // Buscar vencimentos de itens do usuario diferenciando por VAL_ITEM_COMPRA  
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-vencidos-usuario")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarVencidosUsuarioDiferenciado()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var ItensVencidosUsuario = await _context.ItensCompra
                .Include(i => i.PedidoCompra)
                .Where(i =>
                    i.VAL_ITEM_COMPRA < DateTime.Now &&
                    i.PedidoCompra.ID_USUARIO_FK == IdUsuario &&
                    (
                        // Soma das quantidades vendidas para o mesmo produto/lote
                        _context.ItensVenda
                            .Where(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                            .Sum(iv => (decimal?)iv.QTS_ITEM_VENDA) < i.QUANTIDADE_ITEM_COMPRA
                        ||
                        // Caso não exista nenhuma venda para esse produto/lote
                        !_context.ItensVenda.Any(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                    )
                )
                .GroupBy(i => new { i.ID_PRODUTO_FK, i.LOTE_COMPRA })
                .Select(g => g.FirstOrDefault())
                .ToListAsync();

            if (ItensVencidosUsuario == null || !ItensVencidosUsuario.Any())
                return NotFound("Nenhum item vencido encontrado para o usuário atual.");

            return Ok(ItensVencidosUsuario);
        }

        // Buscar vencimento de item do usuario
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-vencidos-usuario/{id_produto}")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarVencidosUsuario(int id_produto)
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var ItensVencidosUsuario = await _context.ItensCompra
                .Include(i => i.PedidoCompra)
                .Where(i =>
                    i.VAL_ITEM_COMPRA < DateTime.Now &&
                    i.PedidoCompra.ID_USUARIO_FK == IdUsuario &&
                    i.ID_PRODUTO_FK == id_produto &&
                    (
                        // Soma das quantidades vendidas para o mesmo produto/lote
                        _context.ItensVenda
                            .Where(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                            .Sum(iv => (decimal?)iv.QTS_ITEM_VENDA) < i.QUANTIDADE_ITEM_COMPRA
                        ||
                        // Caso não exista nenhuma venda para esse produto/lote
                        !_context.ItensVenda.Any(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                    )
                )
                .GroupBy(i => new { i.ID_PRODUTO_FK, i.LOTE_COMPRA })
                .Select(g => g.FirstOrDefault())
                .ToListAsync();

            if (ItensVencidosUsuario == null || !ItensVencidosUsuario.Any())
                return NotFound("Nenhum item vencido encontrado para o usuário atual.");

            return Ok(ItensVencidosUsuario);
        }

        // Buscar vencimentos de itens do usuario do mês  
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-vencidos-mes-usuario")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarVencidosMesUsuario()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // Pela tabela ItensCompra não possuir o ID_USUARIO_FK, é necessário buscar os itens do usuario através do PedidoCompra  
            var ItensVencidosUsuario = await _context.ItensCompra
                .Include(i => i.PedidoCompra)
                .Where(i =>
                    i.VAL_ITEM_COMPRA < DateTime.Now &&
                    i.VAL_ITEM_COMPRA.Value.Month == DateTime.Now.Month &&
                    i.PedidoCompra.ID_USUARIO_FK == IdUsuario &&
                    (
                        // Soma das quantidades vendidas para o mesmo produto/lote
                        _context.ItensVenda
                            .Where(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                            .Sum(iv => (decimal?)iv.QTS_ITEM_VENDA) < i.QUANTIDADE_ITEM_COMPRA
                        ||
                        // Caso não exista nenhuma venda para esse produto/lote
                        !_context.ItensVenda.Any(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                    )
                )
                .GroupBy(i => new { i.ID_PRODUTO_FK, i.LOTE_COMPRA })
                .Select(g => g.FirstOrDefault())
                .ToListAsync();

            if (ItensVencidosUsuario == null || !ItensVencidosUsuario.Any())
                return NotFound("Nenhum item vencido encontrado para o usuário atual.");

            return Ok(ItensVencidosUsuario);
        }

        // Buscar vencimentos de itens do usuario do ano
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-vencidos-ano-usuario")]
        public async Task<ActionResult<IEnumerable<ItensLucro>>> BuscarVencidosAnoUsuario()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // Pela tabela ItensCompra não possuir o ID_USUARIO_FK, é necessário buscar os itens do usuario através do PedidoCompra  
            var ItensVencidosUsuario = await _context.ItensCompra
                .Include(i => i.PedidoCompra)
                .Where(i =>
                    i.VAL_ITEM_COMPRA < DateTime.Now &&
                    i.VAL_ITEM_COMPRA.Value.Year == DateTime.Now.Year &&
                    i.PedidoCompra.ID_USUARIO_FK == IdUsuario &&
                    (
                        // Soma das quantidades vendidas para o mesmo produto/lote
                        _context.ItensVenda
                            .Where(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                            .Sum(iv => (decimal?)iv.QTS_ITEM_VENDA) < i.QUANTIDADE_ITEM_COMPRA
                        ||
                        // Caso não exista nenhuma venda para esse produto/lote
                        !_context.ItensVenda.Any(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                    )
                )
                .GroupBy(i => new { i.ID_PRODUTO_FK, i.LOTE_COMPRA })
                .Select(g => g.FirstOrDefault())
                .ToListAsync();

            if (ItensVencidosUsuario == null || !ItensVencidosUsuario.Any())
                return NotFound("Nenhum item vencido encontrado para o usuário atual.");

            return Ok(ItensVencidosUsuario);
        }

        // Buscar itens faltando 10 dias de vencimento
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-faltando-10-dias-vencimento")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarItensFaltando10DiasVencimento()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var dataLimite = DateTime.Now.AddDays(10);

            var ItensFaltando10Dias = await _context.ItensCompra
                .Include(i => i.PedidoCompra)
                .Where(i =>
                    i.VAL_ITEM_COMPRA.HasValue &&
                    i.VAL_ITEM_COMPRA.Value > DateTime.Now &&
                    i.VAL_ITEM_COMPRA.Value <= dataLimite &&
                    i.PedidoCompra.ID_USUARIO_FK == IdUsuario &&
                    (
                        // Soma das quantidades vendidas para o mesmo produto/lote
                        _context.ItensVenda
                            .Where(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                            .Sum(iv => (decimal?)iv.QTS_ITEM_VENDA) < i.QUANTIDADE_ITEM_COMPRA
                        ||
                        // Caso não exista nenhuma venda para esse produto/lote
                        !_context.ItensVenda.Any(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                    )
                )
                .GroupBy(i => new { i.ID_PRODUTO_FK, i.LOTE_COMPRA })
                .Select(g => g.FirstOrDefault())
                .ToListAsync();

            if (ItensFaltando10Dias == null || !ItensFaltando10Dias.Any())
                return NotFound("Nenhum item faltando 10 dias de vencimento encontrado para o usuário atual.");

            return Ok(ItensFaltando10Dias);
        }

        // Buscar itens faltando 30 dias de vencimento
        [Authorize(Roles = "user")]
        [HttpGet("buscar-itens-faltando-30-dias-vencimento")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarItensFaltando30DiasVencimento()
        {
            int IdUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var dataLimite = DateTime.Now.AddDays(30);

            var ItensFaltando30Dias = await _context.ItensCompra
                .Include(i => i.PedidoCompra)
                .Where(i =>
                    i.VAL_ITEM_COMPRA.HasValue &&
                    i.VAL_ITEM_COMPRA.Value > DateTime.Now &&
                    i.VAL_ITEM_COMPRA.Value <= dataLimite &&
                    i.PedidoCompra.ID_USUARIO_FK == IdUsuario &&
                    (
                        // Soma das quantidades vendidas para o mesmo produto/lote
                        _context.ItensVenda
                            .Where(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                            .Sum(iv => (decimal?)iv.QTS_ITEM_VENDA) < i.QUANTIDADE_ITEM_COMPRA
                        ||
                        // Caso não exista nenhuma venda para esse produto/lote
                        !_context.ItensVenda.Any(iv => iv.ID_PRODUTO_FK == i.ID_PRODUTO_FK && iv.LOTE_VENDA == i.LOTE_COMPRA)
                    )
                )
                .GroupBy(i => new { i.ID_PRODUTO_FK, i.LOTE_COMPRA })
                .Select(g => g.FirstOrDefault())
                .ToListAsync();

            if (ItensFaltando30Dias == null || !ItensFaltando30Dias.Any())
                return NotFound("Nenhum item faltando 30 dias de vencimento encontrado para o usuário atual.");

            return Ok(ItensFaltando30Dias);
        }
    }
}
