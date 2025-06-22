using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Dtos;
using TesouroAzulAPI.Models;
using System.Security.Claims;
using System.Net.Cache;

namespace TesouroAzulAPI.Controllers
{
    [Route("api/Estoque"), ApiController]
    public class EstoqueProdutoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Instanciando contexto do banco de dados
        public EstoqueProdutoController(ApplicationDbContext context) { _context = context; }

        // POSTs
        // Cadastrar estoque
        // Endpoint provavel que não será utilizado
        [Authorize(Roles = "user,admin")]
        [HttpPost("cadastrar-estoque")]
        public async Task<IActionResult> CadastrarEstoque([FromBody] CriarEstoqueDto dto)
        {
            // A criação do estoque será automatica dentro do DB ao gerar um pedido compra
            // Caso não seja criado com o banco de dados pode-se torar este endpoint em um service e aproveita-lo

            // Tratamentos de erros
            if (dto == null) return BadRequest("Preencha os campos corretamente.");
            if (await _context.Produtos.FindAsync(dto.ID_PRODUTO_FK) == null) return NotFound("Produto não encontrado.");
            var estoque = await _context.EstoqueProdutos.FirstOrDefaultAsync(e => e.ID_PRODUTO_FK == dto.ID_PRODUTO_FK);
            if (estoque != null) return BadRequest("Estoque já cadastrado para o produto especificado.");

            // Cria novo estoque
            var novoEstoque = new EstoqueProduto
            {
                ID_PRODUTO_FK = dto.ID_PRODUTO_FK,
                QTD_TOTAL_ESTOQUE = dto.QTD_TOTAL_ESTOQUE,
                VALOR_GASTO_TOTAL_ESTOQUE = dto.VALOR_GASTO_TOTAL_ESTOQUE,
                VALOR_POTENCIAL_VENDA_ESTOQUE = dto.VALOR_POTENCIAL_VENDA_ESTOQUE,
                DATA_ATUALIZACAO_ESTOQUE = DateTime.Now,
                ID_USUARIO_FK = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value) // Obtendo ID do usuario por token
            };

            _context.EstoqueProdutos.Add(novoEstoque);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Estoque cadastrado com sucesso.", Estoque = novoEstoque });
        }

        // Buscar estoque por campo
        [Authorize(Roles = "user,admin")]
        [HttpPost("buscar-por-campo")]
        public async Task<IActionResult> BuscarEstoquePorCampo([FromBody] CamposDinamicoDto filtro)
        {
            // controller vuneravel sem prevenção em tratamentos de erros no filtro
            // buscando idUsuario
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            List<EstoqueProduto> estoqueProduto = new();
            switch (filtro.Campo.ToLower())
            {
                case "quant_total_estoque":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.QTD_TOTAL_ESTOQUE == Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "quant_total_estoque_menor":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.QTD_TOTAL_ESTOQUE < Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "quant_total_estoque_maior":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.QTD_TOTAL_ESTOQUE > Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "valor_gasto_total_estoque":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.VALOR_GASTO_TOTAL_ESTOQUE == Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "valor_gasto_total_estoque_menor":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.VALOR_GASTO_TOTAL_ESTOQUE < Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "valor_gasto_total_estoque_maior":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.VALOR_GASTO_TOTAL_ESTOQUE > Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "valor_potencial_venda_estoque":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.VALOR_POTENCIAL_VENDA_ESTOQUE == Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "valor_potencial_venda_estoque_menor":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.VALOR_POTENCIAL_VENDA_ESTOQUE < Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "valor_potencial_venda_estoque_maior":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.VALOR_POTENCIAL_VENDA_ESTOQUE > Convert.ToDecimal(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "data_atualizacao_estoque":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.DATA_ATUALIZACAO_ESTOQUE == DateTime.Parse(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "data_atualizacao_estoque_maior":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.DATA_ATUALIZACAO_ESTOQUE > DateTime.Parse(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                case "data_atualizacao_estoque_menor":
                    estoqueProduto = await _context.EstoqueProdutos.Where(e => e.DATA_ATUALIZACAO_ESTOQUE < DateTime.Parse(filtro.Valor) && e.ID_USUARIO_FK == idUsuario).ToListAsync();
                    break;
                default:
                    return BadRequest("Somente os campos são permitidos: 'quant_total_estoque', 'quant_total_estoque_menor'," +
                        " 'quant_total_estoque_maior', 'valor_gasto_total_estoque', 'valor_gasto_total_estoque_menor', 'valor_gasto_total_estoque_maior'," +
                        " 'valor_potencial_venda_estoque', 'valor_potencial_venda_estoque_menor', 'valor_potencial_venda_estoque_maior'," +
                        " 'data_atualizacao_estoque', 'data_atualizacao_estoque_maior' e 'data_atualizacao_estoque_menor'");
                    break;
            }
            return Ok(estoqueProduto);

        }
        // GETs
        // Buscar todos os estoques
        [Authorize(Roles = "admin")]
        [HttpGet("buscar-todos-estoques")]
        public async Task<IActionResult> BuscarTodosEstoques()
        {
            var estoqueProdutos = await _context.EstoqueProdutos.ToListAsync();
            return Ok(estoqueProdutos);
        }

        // Buscar estoque por ID_USUARIO
        [Authorize(Roles = "user")]
        [HttpGet("buscar-estoques-por-usuario")]
        public async Task<IActionResult> BuscarEstoquesUsuario()
        {
            // Obtendo ID do usuario por token
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            // Buscando estoques do usuario
            var estoqueProdutos = await _context.EstoqueProdutos
                .Where(e => e.ID_USUARIO_FK == idUsuario)
                .ToListAsync();
            
            // Devolve valores
            if (estoqueProdutos == null || !estoqueProdutos.Any()) return NotFound("Nenhum estoque encontrado para o usuário.");
            return Ok(estoqueProdutos);
        }

        // Buscar estoque por ID_ESTOQUE
        [Authorize(Roles = "user,admin")]
        [HttpGet("buscar-estoque-por-{id_estoque}")]
        public async Task<IActionResult> BuscarPorIdEstoque(int id_estoque)
        {
            var estoqueProduto = await _context.EstoqueProdutos.FindAsync(id_estoque);
            if (estoqueProduto == null) return NotFound("Estoque não encontrado.");

            return Ok(estoqueProduto);
        }

        // Buscar estoque por ID_PRODUTO
        [Authorize(Roles = "user")]
        [HttpGet("buscar-estoque-por-produto-{id_produto}")]
        public async Task<IActionResult> BuscarPorIdProduto(int id_produto)
        {
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var estoqueProduto = await _context.EstoqueProdutos.FirstOrDefaultAsync(e => e.ID_PRODUTO_FK == id_produto && e.ID_USUARIO_FK == idUsuario);

            if (estoqueProduto == null) return NotFound("Estoque não encontrado para o produto especificado.");
            return Ok(estoqueProduto);
        }


        // PATCHs
        // Alterar estoque
        [Authorize(Roles = "user,admin")]
        [HttpPatch("alterar-estoque-por-campo/{id_estoque}")]
        public async Task<IActionResult> AlterarEstoque(int id_estoque, [FromBody] CamposDinamicoDto dto)
        {
            // Verifica se o estoque existe
            var estoqueProduto = await _context.EstoqueProdutos.FindAsync(id_estoque);
            if (estoqueProduto == null) return NotFound("Estoque não encontrado.");
            // Verifica o campo a ser alterado
            switch (dto.Campo.ToLower())
            {
                case "quant_total_estoque":
                    estoqueProduto.QTD_TOTAL_ESTOQUE = Convert.ToDecimal(dto.Valor);
                    break;
                case "valor_gasto_total_estoque":
                    estoqueProduto.VALOR_GASTO_TOTAL_ESTOQUE = Convert.ToDecimal(dto.Valor);
                    break;
                default:
                    return BadRequest("Somente os campos são permitidos: 'quant_total_estoque' e 'valor_gasto_total_estoque'");
            }

            // Atualiza a data de atualização do estoque
            estoqueProduto.DATA_ATUALIZACAO_ESTOQUE = DateTime.Now;
            // Salva as alterações no banco de dados
            _context.EstoqueProdutos.Update(estoqueProduto);
            await _context.SaveChangesAsync();
            return Ok(estoqueProduto);
        }

        // DELETEs
        // Deletar estoque
        [Authorize(Roles = "user,admin")]
        [HttpDelete("deletar-estoque/{id_estoque}")]
        public async Task<IActionResult> DeletarEstoque(int id_estoque)
        {
            // Verifica se o estoque existe
            var estoqueProduto = await _context.EstoqueProdutos.FindAsync(id_estoque);
            if (estoqueProduto == null) return NotFound("Estoque não encontrado.");
            // Remove o estoque do banco de dados
            _context.EstoqueProdutos.Remove(estoqueProduto);
            await _context.SaveChangesAsync();
            return Ok("Estoque deletado com sucesso.");
        }

        // Deletar estoque por id_produto
        [Authorize(Roles = "user,admin")]
        [HttpDelete("deletar-estoque-por-produto/{id_produto}")]
        public async Task<IActionResult> DeletarEstoquePorProduto(int id_produto)
        {
            // Verifica se o estoque existe
            var estoqueProduto = await _context.EstoqueProdutos.FirstOrDefaultAsync(e => e.ID_PRODUTO_FK == id_produto);
            if (estoqueProduto == null) return NotFound("Estoque não encontrado para o produto especificado.");
            // Remove o estoque do banco de dados
            _context.EstoqueProdutos.Remove(estoqueProduto);
            await _context.SaveChangesAsync();
            return Ok("Estoque deletado com sucesso.");
        }
    }
}
