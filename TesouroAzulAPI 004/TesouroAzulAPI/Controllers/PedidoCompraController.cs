using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Dtos;
using TesouroAzulAPI.Models;

namespace TesouroAzulAPI.Controllers
{
    [Route("api/PedidoCompra"), ApiController]
    public class PedidoCompraController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Instandiando o context da ApplicationDbContext para _context
        public PedidoCompraController(ApplicationDbContext context) { _context = context; }

        // DTOs
        // DTO para switch de campos
        public class CamposDtos
        {
            [Required]
            public string Campo { get; set; } = string.Empty;
            [Required]
            public string NovoValor { get; set; } = string.Empty;
        }


        //POSTs
        //Criar Pedido da Compra
        //Neste POST já registra nas tabealas TB_PEDIDO_COMPRA, TB_ITEM_COMPRA, TB_ESTOQUE_PRODUTO (Já será cadastrado por Trigger)
        [Authorize(Roles = "user,admin")]
        [HttpPost("criar-pedido-compra")]
        public async Task<IActionResult> CriarPedidoCompra([FromBody] PedidoCompraCompleto dto)
        {

            // Por se tratar de um tabela que depende de outra, já salva em ambas automaticamente

            var pedido = new PedidosCompra
            {
                ID_USUARIO_FK = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                ID_FORNECEDOR_FK = dto.Pedido.ID_FORNECEDOR.HasValue ? dto.Pedido.ID_FORNECEDOR.Value : (int?)null,
                VALOR_PEDIDO = dto.Pedido.VALOR_VALOR
            };

            _context.PedidosCompra.Add(pedido);
            await _context.SaveChangesAsync();


            // Adicionar busca aqui para descobrir o ID do pedido criado e adicionar na variavel
            int idPedido = pedido.ID_PEDIDO;
            // Variavel de valor para adicionar no VALOR_PEDIDO
            decimal valorPedido = 0;

            var itensSalvo = new List<ItensCompra>();
            if (itensSalvo == null || !dto.Item.Any()) return Ok(new { mensagem = "Compra criado sem produto" , pedido });
            foreach (var item in dto.Item)
            {
                var itemCompra = new ItensCompra
                {
                    ID_PRODUTO_FK = item.ID_PRODUTO_FK,
                    ID_PEDIDO_FK = idPedido, // Usando o ID do pedido criado
                    VAL_ITEM_COMPRA = item.VAL_ITEM_COMPRA.HasValue ? item.VAL_ITEM_COMPRA.Value : (DateTime?)null,
                    LOTE_COMPRA = item.LOTE_COMPRA,
                    QUANTIDADE_ITEM_COMPRA = item.QUANTIDADE_ITEM_COMPRA,
                    N_ITEM_COMPRA = item.N_ITEM_COMPRA,
                    VALOR_TOTAL_ITEM_COMPRA = item.VALOR_TOTAL_ITEM_COMPRA
                };

                valorPedido += item.VALOR_TOTAL_ITEM_COMPRA; // Acumulando o valor total dos itens
                _context.ItensCompra.Add(itemCompra);
                itensSalvo.Add(itemCompra);
            }

            // Verifica se o valor do pedido é zero, se for, não atualiza o valor do pedido

            if (pedido.VALOR_PEDIDO == 0) 
            {            
            // Atualizando o valor do pedido com o total dos itens e salva
                pedido.VALOR_PEDIDO = valorPedido;

                // verifica se o VALOR_PEDIDO é nulo ou zero
                _context.PedidosCompra.Update(pedido);
            }

            
            await _context.SaveChangesAsync();

            // Retorno do JSON com o pedido e seus items
            return Ok(new
            {
                PedidoID = pedido.ID_PEDIDO,
                Itens = itensSalvo.Select(i => new
                {
                    i.ID_ITEM_COMPRA,
                    i.ID_PRODUTO_FK,
                    i.QUANTIDADE_ITEM_COMPRA
                })
            });
        }

        // Inserir item em compra
        [Authorize(Roles = "user,admin")]
        [HttpPost("inserir-itens-em-pedido")]
        public async Task<IActionResult> InserirItemCompra([FromBody] List<ItensCompraDto> dto)
        {
            if (dto == null || !dto.Any()) return BadRequest("Lista de itens não pode ser vazia");
            var itensSalvos = new List<ItensCompra>();
            decimal valorTotal = 0;
            foreach (var item in dto)
            {
                var itemCompra = new ItensCompra
                {
                    ID_PRODUTO_FK = item.ID_PRODUTO_FK,
                    ID_PEDIDO_FK = item.ID_PEDIDO_FK,
                    VAL_ITEM_COMPRA = item.VAL_ITEM_COMPRA ?? null,
                    LOTE_COMPRA = item.LOTE_COMPRA,
                    QUANTIDADE_ITEM_COMPRA = item.QUANTIDADE_ITEM_COMPRA,
                    N_ITEM_COMPRA = item.N_ITEM_COMPRA,
                    VALOR_TOTAL_ITEM_COMPRA = item.VALOR_TOTAL_ITEM_COMPRA
                };
                _context.ItensCompra.Add(itemCompra);
                itensSalvos.Add(itemCompra);

                // Atualiza o valor total do pedido
                valorTotal += item.VALOR_TOTAL_ITEM_COMPRA;

            }
            await _context.SaveChangesAsync();
            // Atualiza o valor do pedido com o total dos itens
            var pedido = await _context.PedidosCompra.FindAsync(dto.First().ID_PEDIDO_FK);
            pedido.VALOR_PEDIDO += valorTotal;
            _context.PedidosCompra.Update(pedido);
            await _context.SaveChangesAsync();

            return Ok(itensSalvos);
        }

        // Buscar pedido por campo do pedido
        [Authorize(Roles = "user,admin")]
        [HttpPost("pedido-compra/Buscar-por-campo")]
        public async Task<ActionResult<IEnumerable<PedidosCompra>>> PedidoBuscarPorCampo([FromBody] CamposDtos filtro)
        {

            List<PedidosCompra> pedidoCampo = new();

            switch (filtro.Campo.ToLower())
            {
                case "fornecedor_pedido":
                    pedidoCampo = await _context.PedidosCompra.Where(p => p.ID_FORNECEDOR_FK == Convert.ToInt16(filtro.NovoValor)).ToListAsync();
                    if (pedidoCampo == null) return NotFound("Fornecedor não cadastrado"); // Verificar aqui mais tarde para analisar se quebra lógica

                    break;
                case "data_pedido":
                    if (!DateTime.TryParse(filtro.NovoValor, out DateTime dataPedido)) return BadRequest("Novo Valor para Data deve ser uma data válida.");
                    pedidoCampo = await _context.PedidosCompra.Where(p => p.DATA_PEDIDO == Convert.ToDateTime(filtro.NovoValor)).ToListAsync();

                    break;
                // Retorna os pedidos com data anterior
                case "data_anterior_pedido":
                    if(!DateTime.TryParse(filtro.NovoValor, out DateTime dataAnterior)) return BadRequest("Novo Valor para Data deve ser uma data válida.");
                    pedidoCampo = await _context.PedidosCompra.Where(p => p.DATA_PEDIDO < Convert.ToDateTime(filtro.NovoValor)).ToListAsync();

                    break;
                // Retorna os pedidos com data posterior
                case "data_posterior_pedido":
                    if (!DateTime.TryParse(filtro.NovoValor, out DateTime dataPosterior)) return BadRequest("Novo Valor para Data deve ser uma data válida.");
                    pedidoCampo = await _context.PedidosCompra.Where(p => p.DATA_PEDIDO > Convert.ToDateTime(filtro.NovoValor)).ToListAsync();
                    break;
                case "valor_pedido":
                    pedidoCampo = await _context.PedidosCompra.Where(p => p.VALOR_PEDIDO == Convert.ToDecimal(filtro.NovoValor)).ToListAsync();

                    break;
                case "valor_menor_pedido":
                    pedidoCampo = await _context.PedidosCompra.Where(p => p.VALOR_PEDIDO < Convert.ToDecimal(filtro.NovoValor)).ToListAsync();

                    break;
                case "valor_maior_pedido":
                    pedidoCampo = await _context.PedidosCompra.Where(p => p.VALOR_PEDIDO > Convert.ToDecimal(filtro.NovoValor)).ToListAsync();
                    break;
                default:
                    return BadRequest("Somente os campos são aceitos : fornecedor_pedido, data_pedido, data_anterior_pedido, data_posterior_pedido, valor_pedido," +
                        "valor_menor_pedido, valor_maior_pedido");
                    break;
            }

            if (!pedidoCampo.Any()) return NotFound("Valor não encontrado");
            return Ok(pedidoCampo);
        }
        // Buscar por campo do item
        [Authorize(Roles ="user,admin")]
        [HttpPost("item-compra/Buscar-por-campo")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> ItemBuscarPorCampo(int id_pedido, [FromBody] CamposDtos filtro)
        {
            // tratamento de erro
            if (id_pedido == 0) return BadRequest("ID do pedido não pode ser 0");
            List<ItensCompra> itemCampo = new();
            switch (filtro.Campo.ToLower())
            {
                case "produto_item":
                    itemCampo = await _context.ItensCompra.Where(i => i.ID_PRODUTO_FK == Convert.ToInt16(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                case "lote_item":
                    itemCampo = await _context.ItensCompra.Where(i => i.LOTE_COMPRA == filtro.NovoValor && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                case "quantidade_item":
                    itemCampo = await _context.ItensCompra.Where(i => i.QUANTIDADE_ITEM_COMPRA == Convert.ToDecimal(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                case "n_item_compra":
                    itemCampo = await _context.ItensCompra.Where(i => i.N_ITEM_COMPRA == Convert.ToInt16(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                case "val_item":
                    if (!DateTime.TryParse(filtro.NovoValor, out DateTime valItem)) return BadRequest("Novo Valor para Data deve ser uma data válida.");
                    itemCampo = await _context.ItensCompra.Where(i => i.VAL_ITEM_COMPRA == Convert.ToDateTime(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                // Item vencido
                case "item_vencido":
                    filtro.NovoValor = DateTime.Now.ToString("yyyy-MM-dd");
                    itemCampo = await _context.ItensCompra.Where(i => i.VAL_ITEM_COMPRA < Convert.ToDateTime(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                // Item não vencido
                case "item_nao_vencido":
                    filtro.NovoValor = DateTime.Now.ToString("yyyy-MM-dd");
                    itemCampo = await _context.ItensCompra.Where(i => i.VAL_ITEM_COMPRA > Convert.ToDateTime(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                case "valor_total_item":
                    itemCampo = await _context.ItensCompra.Where(i => i.VALOR_TOTAL_ITEM_COMPRA == Convert.ToDecimal(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                case "valor_total_item_maior":
                    itemCampo = await _context.ItensCompra.Where(i => i.VALOR_TOTAL_ITEM_COMPRA > Convert.ToDecimal(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                case "valor_total_item_menor":
                    itemCampo = await _context.ItensCompra.Where(i => i.VALOR_TOTAL_ITEM_COMPRA < Convert.ToDecimal(filtro.NovoValor) && i.ID_PEDIDO_FK == id_pedido).ToListAsync();
                    break;
                default:
                    return BadRequest("Somente os campos são aceitos : produto_item, lote_item, quantidade_item, n_item_compra, val_item, item_vencido, item_nao_vencido, valor_total_item, valor_total_item_maior, valor_total_item_menor ");
            }
            if (!itemCampo.Any()) return NotFound("Item não encontrado");
            return Ok(itemCampo);
        }
        //GETs
        //Buscar Compras Pedido
        [Authorize(Roles ="admin")]
        [HttpGet("buscar-todos-pedidos")]
        public async Task<ActionResult<IEnumerable<PedidosCompra>>> BuscarComprasPedido()
        {
            var pedido = await _context.PedidosCompra.ToListAsync();
            if (!pedido.Any()) return NotFound("Nenhum pedido encontrado");
            return Ok(pedido);
        }

        //Bucsar Itens das compras por pedido
        [Authorize(Roles ="user,admin")]
        [HttpGet("Itens/{id_pedido}")]
        public async Task<ActionResult<IEnumerable<ItensCompra>>> BuscarItensCompraPorPedido(int id_pedido)
        {
            var itens = await _context.ItensCompra.Where(i => i.ID_PEDIDO_FK == id_pedido).ToListAsync();
            if (!itens.Any()) return NotFound("Nenhum item encontrado para o pedido");
            return Ok(itens);
        }

        //Buscar Compras Pedido por usuario
        [Authorize(Roles ="user,admin")]
        [HttpGet("buscar-pedidos-usuario")]
        public async Task<ActionResult<IEnumerable<PedidosCompra>>> BuscarComprasPedidoPorUsuario()
        {
            // Buscando id
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // tratamento de erro
            var pedido = await _context.PedidosCompra.Where(p => p.ID_USUARIO_FK == idUsuario).ToListAsync();
            if (!pedido.Any()) return NotFound("Nenhum pedido encontrado para este usuário");
            return Ok(pedido);
        }

        //PATCHs
        //Alterar Pedido Compra {campo}
        [Authorize(Roles = "user,admin")]
        [HttpPatch("alterar-pedido-por-campo/{id_pedido}")]
        public async Task<IActionResult> AlterarPedidoCompra(int id_pedido, [FromBody] CamposDtos dto)
        {
            // tratamento de erro
            var pedido = await _context.PedidosCompra.FindAsync(id_pedido);
            if (pedido == null) return NotFound("Pedido não encontrado");
            // Inicia busca dinamica
            switch (dto.Campo.ToLower())
            {
                case "fornecedor_pedido":
                    if (!int.TryParse(dto.NovoValor, out int fornecedorId)) return BadRequest("Novo Valor para Fornecedor deve ser um número válido.");
                    if (!_context.Fornecedores.Any(f => f.ID_FORNECEDOR == fornecedorId)) return NotFound("Fornecedor não encontrado");
                    pedido.ID_FORNECEDOR_FK = Convert.ToInt16(dto.NovoValor);
                    break;
                case "data_pedido":
                    if (DateTime.TryParse(dto.NovoValor, out DateTime dataVal)) { pedido.DATA_PEDIDO = dataVal; }
                    else { return BadRequest("Novo Valor para Data deve ser uma data válida."); }
                    break;
                case "valor_pedido":
                    pedido.VALOR_PEDIDO = Convert.ToDecimal(dto.NovoValor);
                    break;
                default:
                    return BadRequest("Campos permitidos : fornecedor_pedido, data_pedido, valor_pedido");
            }
            _context.PedidosCompra.Update(pedido);
            await _context.SaveChangesAsync();
            return Ok(pedido);
        }
        // Alterar Itens Compra {campo}
        [Authorize(Roles ="user,admin")]
        [HttpPatch("alterar-item-do-pedido/{id_item}")]
        public async Task<IActionResult> AlterarItemCompra(int id_item, [FromBody] CamposDtos dto)
        {
            // tratamento de erro
            var item = await _context.ItensCompra.FindAsync(id_item);
            if (item == null) return NotFound("Item não encontrado");
            switch (dto.Campo.ToLower())
            {
                case "produto_item":
                    item.ID_PRODUTO_FK = Convert.ToInt16(dto.NovoValor);
                    break;
                case "lote_item":
                    item.LOTE_COMPRA = dto.NovoValor;
                    break;
                case "quantidade_item":
                    item.QUANTIDADE_ITEM_COMPRA = Convert.ToDecimal(dto.NovoValor);
                    break;
                case "n_item_compra":
                    item.N_ITEM_COMPRA = Convert.ToInt16(dto.NovoValor);
                    break;
                case "val_total_item":
                    item.VALOR_TOTAL_ITEM_COMPRA = Convert.ToDecimal(dto.NovoValor);
                    break;
                case "estado_item":
                    item.ESTADO_ITEM_COMPRA = Convert.ToString(dto.NovoValor);
                    break;
                default:
                    return BadRequest("Campos permitidos : produto_item, lote_item, quantidade_item, n_item_compra, val_total_item, estado_item");
            }
            _context.ItensCompra.Update(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        //DElETEs
        //Deletar pedido compra
        [Authorize(Roles ="user,admin")]
        [HttpDelete("{id_pedido}")]
        public async Task<IActionResult> DeletarPedidoCompra(int id_pedido)
        {
            // tratamento de erro
            var pedido = await _context.PedidosCompra.FindAsync(id_pedido);
            if (pedido == null) return NotFound("Pedido não encontrado");
            _context.PedidosCompra.Remove(pedido);
            await _context.SaveChangesAsync();
            return Ok("Pedido deletado com sucesso");

        }
        // Deletar Itens Compra
        [Authorize(Roles ="user,admin")]
        [HttpDelete("Itens/{id_item}")]
        public async Task<IActionResult> DeletarITemCompra(int id_item)
        {
            var id_item_compra = await _context.ItensCompra.FindAsync(id_item);
            if (id_item_compra == null) return NotFound("Item não encontrado");

            // atualiza o valor do pedido
            var pedido = await _context.PedidosCompra.FindAsync(id_item_compra.ID_PEDIDO_FK);
            if (pedido == null) return NotFound("Pedido não encontrado");
            pedido.VALOR_PEDIDO -= id_item_compra.VALOR_TOTAL_ITEM_COMPRA;
            _context.PedidosCompra.Update(pedido);

            _context.ItensCompra.Remove(id_item_compra);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Item removido com sucesso" });
        }
    }
}
