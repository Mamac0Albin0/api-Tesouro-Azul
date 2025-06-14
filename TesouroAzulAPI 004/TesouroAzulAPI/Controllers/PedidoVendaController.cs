using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Models;
using TesouroAzulAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TesouroAzulAPI.Controllers
{
    [Route("api/PedidoVenda"), ApiController]
    public class PedidoVendaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Instanciando com o banco
        public PedidoVendaController(ApplicationDbContext context) { _context = context; }

        // POSTs
        // Criar Pedido Venda
        // Neste Post já adiciona os ItensVenda associados ao PedidoVenda
        [Authorize(Roles ="user,admin")]
        [HttpPost("criar-pedido-venda")]
        public async Task<IActionResult> CriarPedidoVenda([FromBody] Dtos.PedidoVendaCompleto dto)
        {
            var pedidoVenda = new PedidosVenda
            {
                ID_USUARIO_FK = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                VALOR_PEDIDO_VENDA = dto.Pedido.VALOR_PEDIDO_VENDA ?? 0.00m // Valor padrão se não for informado
            };

            _context.PedidosVenda.Add(pedidoVenda);
            await _context.SaveChangesAsync();

            // Adicionando os ItensVenda associados ao PedidoVenda

            int idPedidoVenda = pedidoVenda.ID_PEDIDO_VENDA;

            var itensSalvo = new List<ItensVenda>();
            foreach (var item in dto.Item)
            {
                var itemVenda = new ItensVenda
                {
                    ID_PRODUTO_FK = item.ID_PRODUTO_FK,
                    ID_PEDIDO_VENDA_FK = idPedidoVenda,
                    LOTE_VENDA = item.LOTE_VENDA,
                    QTS_ITEM_VENDA = item.QTS_ITEM_VENDA,
                    N_ITEM_VENDA = item.N_ITEM_VENDA,
                    DESCONTO_ITEM_VENDA = item.DESCONTO_ITEM_VENDA ?? 0.00m, // Valor padrão se não for informado
                    VALOR_TOTAL_ITEM_VENDA = item.VALOR_TOTAL_ITEM_VENDA
                };

                _context.ItensVenda.Add(itemVenda);
                itensSalvo.Add(itemVenda);

                // Atualizando a tabela de ItensCompra no campo "ESTADO_ITEM_COMPRA"

            }
            await _context.SaveChangesAsync();

            // Verifica quantidade no estoque e retorna aviso que o estoque ficou negativo
            // Adicionar logica mais tarde aqui
            
            return Ok(new
            {
                PedidoVenda = pedidoVenda,
                ItensVenda = itensSalvo
            });

        }

        // Criar Item Venda
        // Adiciona Item associado ao Pedido venda 
        [Authorize(Roles ="user,admin")]
        [HttpPost("inserir-itens-em-pedido-venda")]
        public async Task<IActionResult> InserirItensEmPedidoComrpa(int id_pedido_venda, [FromBody] List<ItensVendaDto> dto)
        {
            // tratamentos de erro
            if (dto == null || !dto.Any()) return BadRequest("Nenhum item de venda fornecido.");
            var itensSalvos = new List<ItensVenda>();
            foreach (var item in dto)
            {
                // Tratamentos de erro dos itens
                // Verifica se o produto existe
                var produto = await _context.Produtos.FindAsync(item.ID_PRODUTO_FK);
                if (produto == null) return NotFound($"Produto com ID {item.ID_PRODUTO_FK} não encontrado.");

                // Cria o item de venda
                var itemVenda = new ItensVenda
                {
                    ID_PRODUTO_FK = item.ID_PRODUTO_FK,
                    ID_PEDIDO_VENDA_FK = id_pedido_venda,
                    LOTE_VENDA = item.LOTE_VENDA,
                    QTS_ITEM_VENDA = item.QTS_ITEM_VENDA,
                    N_ITEM_VENDA = item.N_ITEM_VENDA,
                    DESCONTO_ITEM_VENDA = item.DESCONTO_ITEM_VENDA ?? 0.00m, // Valor padrão se não for informado
                    VALOR_TOTAL_ITEM_VENDA = item.VALOR_TOTAL_ITEM_VENDA
                };
                _context.ItensVenda.Add(itemVenda);
                itensSalvos.Add(itemVenda);
            }

            await _context.SaveChangesAsync();
            return Ok(itensSalvos);

        }

        
        // Buscar Pedido Venda por campo
        [Authorize(Roles = "user,admin")]
        [HttpPost("pedido-venda/buscar-por-campo")]
        public async Task<IActionResult> BuscarPedidoVendaPorCampo([FromBody] CamposDinamicoDto filtro)
        {
            // tratamentos de erro
            if (string.IsNullOrEmpty(filtro.Valor) || string.IsNullOrEmpty(filtro.Campo)) return BadRequest("Campo e valor são obrigatórios para a busca.");

            // variavel de retorno
            List<PedidosVenda> pedidoVenda = new();
            switch (filtro.Campo.ToLower())
            {
                case "id_pedido_venda":
                    pedidoVenda = await _context.PedidosVenda.Where(p => p.ID_PEDIDO_VENDA == Convert.ToInt32(filtro.Valor))
                        .Include(p => p.Usuario)
                        .ToListAsync();
                    if (!pedidoVenda.Any()) return BadRequest("Nenhum pedido de venda encontrado com o ID especificado.");

                    break;
                case "data_pedido_venda":
                    if (DateTime.TryParse(filtro.Valor, out DateTime dataPedido))
                    {
                        pedidoVenda = await _context.PedidosVenda.Where(p => p.DATA_PEDIDO_VENDA.Date == dataPedido.Date)
                            .Include(p => p.Usuario)
                            .ToListAsync();
                    }
                    else
                    {
                        return BadRequest("Formato de data inválido.");
                    }
                    break;
                case "data_pedido_venda_posterior":
                    if (DateTime.TryParse(filtro.Valor, out DateTime dataPedidoPosterior))
                    {
                        pedidoVenda = await _context.PedidosVenda.Where(p => p.DATA_PEDIDO_VENDA > dataPedidoPosterior)
                            .Include(p => p.Usuario)
                            .ToListAsync();
                    }
                    else
                    {
                        return BadRequest("Formato de data inválido.");
                    }
                    break;
                case "data_pedido_venda_anterior":
                    if (DateTime.TryParse(filtro.Valor, out DateTime dataPedidoAnterior))
                    {
                        pedidoVenda = await _context.PedidosVenda.Where(p => p.DATA_PEDIDO_VENDA < dataPedidoAnterior)
                            .Include(p => p.Usuario)
                            .ToListAsync();
                    }
                    else
                    {
                        return BadRequest("Formato de data inválido.");
                    }
                    break;
                case "valor_pedido_venda":
                    if (decimal.TryParse(filtro.Valor, out decimal valorPedido))
                    {
                        pedidoVenda = await _context.PedidosVenda.Where(p => p.VALOR_PEDIDO_VENDA == valorPedido)
                            .Include(p => p.Usuario)
                            .ToListAsync();
                    }
                    else
                    {
                        return BadRequest("Formato de valor inválido.");
                    }
                    break;
                case "valor_pedido_venda_maior":
                    if (decimal.TryParse(filtro.Valor, out decimal valorPedidoMaior))
                    {
                        pedidoVenda = await _context.PedidosVenda.Where(p => p.VALOR_PEDIDO_VENDA > valorPedidoMaior)
                            .Include(p => p.Usuario)
                            .ToListAsync();
                    }
                    else
                    {
                        return BadRequest("Formato de valor inválido.");
                    }
                    break;
                case "valor_pedido_venda_menor":
                    if (decimal.TryParse(filtro.Valor, out decimal valorPedidoMenor))
                    {
                        pedidoVenda = await _context.PedidosVenda.Where(p => p.VALOR_PEDIDO_VENDA < valorPedidoMenor)
                            .Include(p => p.Usuario)
                            .ToListAsync();
                    }
                    else
                    {
                        return BadRequest("Formato de valor inválido.");
                    }
                    break;
                default:
                    return BadRequest("Campo inválido. Utilize 'id_pedido_venda', 'data_pedido_venda', 'data_pedido_venda_posterior', 'data_pedido_venda_anterior', 'valor_pedido_venda', 'valor_pedido_venda_maior' ou 'valor_pedido_venda_menor'.");

            }
            return Ok(pedidoVenda);

        }



        // Buscar Itens Venda por campo
        [Authorize(Roles = "user,admin")]
        [HttpPost("itens-venda/buscar-por-campo")]
        public async Task<IActionResult> BuscarItensVendaPorCampo([FromBody] CamposDinamicoDto filtro)
        {
            // tratamentos de erro
            if (string.IsNullOrEmpty(filtro.Valor) || string.IsNullOrEmpty(filtro.Campo)) return BadRequest("Campo e valor são obrigatórios para a busca.");
            // variavel de retorno
            List<ItensVenda> itensVenda = new();
            switch (filtro.Campo.ToLower())
            {
                case "id_item_venda":
                    itensVenda = await _context.ItensVenda.Where(i => i.ID_ITEM_VENDA == Convert.ToInt32(filtro.Valor))
                        .Include(i => i.Produto)
                        .Include(i => i.PedidoVenda)
                        .ToListAsync();
                    break;
                case "id_pedido_venda_fk":
                    itensVenda = await _context.ItensVenda.Where(i => i.ID_PEDIDO_VENDA_FK == Convert.ToInt32(filtro.Valor))
                        .Include(i => i.Produto)
                        .Include(i => i.PedidoVenda)
                        .ToListAsync();
                    break;
                case "n_item_venda":
                    itensVenda = await _context.ItensVenda.Where(i => i.N_ITEM_VENDA == Convert.ToInt32(filtro.Valor))
                        .Include(i => i.Produto)
                        .Include(i => i.PedidoVenda)
                        .ToListAsync();
                    break;
                case "valor_total_item_venda":
                    if (decimal.TryParse(filtro.Valor, out decimal valorTotalItem))
                    {
                        itensVenda = await _context.ItensVenda.Where(i => i.VALOR_TOTAL_ITEM_VENDA == valorTotalItem)
                            .Include(i => i.Produto)
                            .Include(i => i.PedidoVenda)
                            .ToListAsync();
                    }
                    else
                    {
                        return BadRequest("Formato de valor inválido.");
                    }
                    break;
                default:
                    return BadRequest("Campo inválido. Utilize 'id_item_venda', 'id_pedido_venda_fk', 'n_item_venda' ou 'valor_total_item_venda'.");
            }
            return Ok(itensVenda);
        }

        // GETs
        // Buscar todos os pedidos de venda
        [Authorize(Roles = "admin")]
        [HttpGet("buscar-todos-pedidos")]
        public async Task<IActionResult> BuscarTodosPedidosVenda()
        {
            var pedidosVenda = await _context.PedidosVenda
                .Include(p => p.Usuario)
                .ToListAsync();
            return Ok(pedidosVenda);
        }

        // Buscar pedidos venda por id_usuario
        [Authorize(Roles = "user,admin")] // Remover admin posteriormente
        [HttpGet("buscar-pedidos-por-usuario")]
        public async Task<IActionResult> BuscarPedidosVendaPorUsuario()
        {
            // Obtendo o ID do usuário autenticado
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var pedidosVenda = await _context.PedidosVenda
                .Where(p => p.ID_USUARIO_FK == idUsuario)
                .Include(p => p.Usuario)
                .ToListAsync();
            return Ok(pedidosVenda);
        }

        // Buscar todos os itens de venda
        [Authorize(Roles = "admin")]
        [HttpGet("buscar-todos-itens-venda")]
        public async Task<IActionResult> BuscarTodosItensVenda()
        {
            var itensVenda = await _context.ItensVenda
                .Include(i => i.Produto)
                .Include(i => i.PedidoVenda)
                .ToListAsync();
            return Ok(itensVenda);
        }

        // Buscar itens venda por id_usuario
        [Authorize(Roles = "user,admin")] // Remover admin posteriormente
        [HttpGet("buscar-itens-venda-por-usuario")]
        public async Task<IActionResult> BuscarItensVendaPorUsuario()
        {
            // Obtendo o ID do usuário autenticado
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var itensVenda = await _context.ItensVenda
                .Where(i => i.PedidoVenda.ID_USUARIO_FK == idUsuario)
                .Include(i => i.Produto)
                .Include(i => i.PedidoVenda)
                .ToListAsync();
            return Ok(itensVenda);
        }

        // PATCHs
        // Alterar Pedido Venda por id_pedido_venda
        [Authorize(Roles = "user,admin")]
        [HttpPatch("alterar-pedido-por-campo/{id_pedido_venda}")]
        public async Task<IActionResult> AlterarPedidoVendaCompra(int id_pedido_venda, [FromBody] CamposDinamicoDto dto)
        {
            // tratamentos de erros
            if (dto == null || string.IsNullOrEmpty(dto.Campo) || string.IsNullOrEmpty(dto.Valor)) return BadRequest("Campo e valor são obrigatórios para a alteração.");

            var pedidoVenda = await _context.PedidosVenda.FindAsync(id_pedido_venda);
            if (pedidoVenda == null) return NotFound($"Pedido de venda com ID {id_pedido_venda} não encontrado.");

            switch (dto.Campo.ToLower())
            {
                case "data_pedido":
                    if (DateTime.TryParse(dto.Valor, out DateTime dataPedido))
                    {
                        pedidoVenda.DATA_PEDIDO_VENDA = dataPedido;
                    }
                    else
                    {
                        return BadRequest("Formato de data inválido.");
                    }
                    break;
                case "valor_pedido_venda":
                    pedidoVenda.VALOR_PEDIDO_VENDA = decimal.TryParse(dto.Valor, out decimal valorPedido) ? valorPedido : 0.00m;
                    break;
                default:
                    return BadRequest("Somente os campos são permitidos : data_pedido , valor_pedido_venda");
                    break;
            }

            _context.PedidosVenda.Update(pedidoVenda);
            await _context.SaveChangesAsync();
            return Ok(pedidoVenda);
        }

        // Alterar Item Venda por id_item_venda
        [Authorize(Roles = "user,admin")]
        [HttpPatch("alterar-item-do-pedido-{id}")]
        public async Task<IActionResult> AlterarItemVenda(int id_item_venda, [FromBody] CamposDinamicoDto dto)
        {
            var item = await _context.ItensVenda.FindAsync(id_item_venda);

            // Tratamentos de erro
            if (dto == null) return BadRequest("");
            if (item == null) return BadRequest("Item não encontrado no pedido");

            switch (dto.Campo.ToLower())
            {
                // Atualiza quantidade e valor total
                case "qts_item_venda":
                    if (decimal.TryParse(dto.Valor, out decimal qtsItemVenda))
                    {
                        item.QTS_ITEM_VENDA = qtsItemVenda;

                        // Realiza busca pelo valor unitario do produto
                        var produto = await _context.Produtos.FindAsync(item.ID_PRODUTO_FK);
                        if (produto == null) return NotFound($"Produto com ID {item.ID_PRODUTO_FK} não encontrado.");

                        decimal valorUnitarioProd = produto.VALOR_PRODUTO; // Supondo que o produto tem um preço

                        // Atualizando o valor total do item
                        item.VALOR_TOTAL_ITEM_VENDA = (item.QTS_ITEM_VENDA * valorUnitarioProd) - item.DESCONTO_ITEM_VENDA; // Supondo que o produto tem um preço
                    }
                    else
                    {
                        return BadRequest("Formato de quantidade inválido.");
                    }
                    break;
                // Atualiza desconto e valor total
                case "desconto_item_venda":
                    if (decimal.TryParse(dto.Valor, out decimal descontoItemVenda))
                    {
                        // Atualiza valor total
                        item.VALOR_TOTAL_ITEM_VENDA += item.DESCONTO_ITEM_VENDA; // Adiciona o desconto antigo para corrigir o valor total
                        item.VALOR_TOTAL_ITEM_VENDA -= descontoItemVenda; // Atualiza o valor total subtraindo o novo desconto
                        item.DESCONTO_ITEM_VENDA = descontoItemVenda;
                    }
                    else
                    {
                        return BadRequest("Formato de desconto inválido.");
                    }
                    break;
                case "valor_total_item_venda":
                    if (decimal.TryParse(dto.Valor, out decimal valorTotalItemVenda))
                    {
                        item.VALOR_TOTAL_ITEM_VENDA = valorTotalItemVenda;
                    }
                    else
                    {
                        return BadRequest("Formato de valor total inválido.");
                    }
                    break;
                default:
                    return BadRequest("Somente os campos são permitidos : qts_item_venda , desconto_item_venda , valor_total_item_venda");
                    break;
            }
            _context.ItensVenda.Update(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        // DELETEs
        // Deletar Pedido Venda por id_usuario
        [Authorize(Roles = "user,admin")]
        [HttpDelete("deletar-pedido-venda/{id}")]
        public async Task<IActionResult> DeletarPedidoVenda(int id)
        {
            var pedidoVenda = await _context.PedidosVenda.FindAsync(id);
            if (pedidoVenda == null) return NotFound($"Pedido de venda com ID {id} não encontrado.");
            _context.PedidosVenda.Remove(pedidoVenda);
            await _context.SaveChangesAsync();
            return Ok($"Pedido de venda com ID {id} deletado com sucesso.");
        }

        // Deletar Item Venda por id_item_venda
        // Atualiza o valor total do pedido com trigger no DB, e atualiza o estoque
        [Authorize(Roles = "user,admin")]
        [HttpDelete("deletar-item-venda/{id}")]
        public async Task<IActionResult> DeletarItemVenda(int id)
        {
            var itemVenda = await _context.ItensVenda.FindAsync(id);
            if (itemVenda == null) return NotFound($"Item de venda com ID {id} não encontrado.");
            _context.ItensVenda.Remove(itemVenda);
            await _context.SaveChangesAsync();
            return Ok($"Item de venda com ID {id} deletado com sucesso.");
        }
    }
}
