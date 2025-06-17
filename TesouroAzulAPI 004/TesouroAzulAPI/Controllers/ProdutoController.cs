using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks.Dataflow;
using Microsoft.EntityFrameworkCore;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Dtos;
using TesouroAzulAPI.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Numerics;
using Microsoft.OpenApi.Any;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TesouroAzulAPI.Controllers
{
    [Route("api/Produtos"), ApiController]
    public class ProdutoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // instanciando o contexto do banco
        public ProdutoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTOs
        // DTO para switch de campos
        public class camposDtos
        {
            public string Campo { get; set; } = string.Empty;
            public string NovoValor { get; set; } = string.Empty;
        }

        //POSTs
        //Cadastrar Produto
        
        [Authorize(Roles = "user,admin")]
        [HttpPost("/cadastrar-produto")]
        public async Task<IActionResult> CadastrarProduto([FromBody] CadastrarProdutoDto produtoDto)
        {
            var produto = new Produto
            {
                ID_USUARIO_FK = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                COD_PRODUTO = produtoDto.COD_PRODUTO,
                NOME_PRODUTO = produtoDto.NOME_PRODUTO,
                VALOR_PRODUTO = produtoDto.VALOR_PRODUTO,
                TIPO_PRODUTO = Convert.ToString(produtoDto.TIPO_PRODUTO),
                IMG_PRODUTO =  String.IsNullOrEmpty(produtoDto.IMG_PRODUTO) ? null : Convert.FromBase64String(produtoDto.IMG_PRODUTO)
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return Ok("Produto cadastrado com sucesso");
            
        }

        //Buscar Produto {campo}
        [Authorize(Roles = "user")]
        [HttpPost("/buscar-produtos-por-campo")]
        public async Task<ActionResult<IEnumerable<Produto>>> BurcarPorCampo([FromBody] camposDtos filtro)
        {
            // tratamento de erro
            int id_usuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuario = await _context.Usuarios.FindAsync(id_usuario);
            if (usuario == null) return NotFound("Usuario não encontrado");
            if (string.IsNullOrEmpty(filtro.Campo) || string.IsNullOrEmpty(filtro.NovoValor)) return BadRequest("Campos permitidos : cod_produto, nome_produto, tipo_produto, data_val_produto, não_vencidos, vencidos");

            switch (filtro.Campo)
            {
                case "cod_produto":
                    var produtoCod = await _context.Produtos.Where(p => p.COD_PRODUTO == filtro.NovoValor && p.ID_USUARIO_FK == id_usuario).ToListAsync();
                    if (!produtoCod.Any()) return NotFound("Código não encontrado");
                    return Ok(produtoCod);
                    break;
                case "nome_produto":
                    var produtoNome = await _context.Produtos.Where(p => p.NOME_PRODUTO == filtro.NovoValor && p.ID_USUARIO_FK == id_usuario).ToListAsync();
                    if (!produtoNome.Any()) return NotFound("Nome não encontrado");
                    return Ok(produtoNome);
                    break;
                case "tipo_produto":
                    var produtoTipo = await _context.Produtos.Where(p => p.TIPO_PRODUTO == filtro.NovoValor && p.ID_USUARIO_FK == id_usuario).ToListAsync();
                    if (!produtoTipo.Any()) return NotFound("Tipo não encontrado");
                    return Ok(produtoTipo);
                    break;
                default:
                    return BadRequest("Campos permitidos : cod_produto, nome_produto, tipo_produto");
            }


        }


        // Buscar por nome similiar
        [Authorize(Roles = "user")]
        [HttpPost("/buscar-produtos-por-nome-similar")]
        public async Task<IActionResult> BuscarPorNomeSimilar([FromBody] string nome)
        {
            // tratamento de erro
            if (string.IsNullOrEmpty(nome)) return BadRequest("Nome não pode ser nulo ou vazio");
            // busca id pelo token
            int id_usuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var usuario = await _context.Usuarios.FindAsync(id_usuario);
            if (usuario == null) return NotFound("Usuario não encontrado");
            var produtos = await _context.Produtos
                .Where(p => p.NOME_PRODUTO.Contains(nome) && p.ID_USUARIO_FK == id_usuario)
                .ToListAsync();
            if (!produtos.Any()) return NotFound("Nenhum produto encontrado com esse nome similar");
            return Ok(produtos);

        }

        //GETs
        //Buscar Produtos
        [Authorize(Roles = "admin")] 
        [HttpGet("/buscar-todos-protudos")]
        public async Task<ActionResult<IEnumerable<Produto>>> BuscarProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        // Bucar Produtos {id_usuario}
        [Authorize(Roles = "user")]
        [HttpGet("buscar-todos-produtos-users")]
        public async Task<ActionResult<IEnumerable<Produto>>> BuscarProdutoIdUsuario()
        {
            int id_usuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var produto = await _context.Produtos.Where(p => p.ID_USUARIO_FK == id_usuario).ToListAsync();
            if (!produto.Any()) return NotFound("Produto não encontrado para este usuario");
            return Ok(produto);
        }

        //Buscar Produto {id}
        [Authorize(Roles = "user,admin")]
        [HttpGet("buscar-produto/{id}")]
        public async Task<ActionResult<IEnumerable<Produto>>> BuscarProdutoId(int id)
        {
            // Instanciando id user
            int id_usuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var produto = await _context.Produtos.Where(p => p.ID_PRODUTO == id && p.ID_USUARIO_FK == id_usuario).ToListAsync();
            if (produto == null) return NotFound("Produto não encontrado");
            return Ok(produto);
        }

        //PATCHs
        //Alterar Produto {campo}
        [Authorize(Roles = "user,admin")]
        [HttpPatch("alterar-produto-por-campo-{id}")]
        public async Task<IActionResult> AlterarProduto(int id, [FromBody] camposDtos campo)
        {
            try
            {
                // Instanciando user id
                int id_usuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                // tratamento de erro
                var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.ID_PRODUTO == id && p.ID_USUARIO_FK == id_usuario);
                if (produto == null) return NotFound("Produto não encontrado");

                switch (campo.Campo)
                {
                    case "cod_produto":
                        produto.COD_PRODUTO = campo.NovoValor;
                        break;
                    case "nome_produto":
                        produto.NOME_PRODUTO = campo.NovoValor;
                        break;
                    case "tipo_produto":
                        produto.TIPO_PRODUTO = campo.NovoValor;
                        break;
                    case "valor_produto":
                        produto.VALOR_PRODUTO = Convert.ToDecimal(campo.NovoValor);
                        break;
                    default:
                        return BadRequest("Campos permitidos : cod_produto, nome_produto, tipo_produto, valor_produto");
                        break;
                }

                _context.Produtos.Update(produto);
                await _context.SaveChangesAsync();
                return Ok(produto);
            }
            catch(Exception ex)
            {
                return BadRequest("Ocorreu um erro no sistema : " + ex.Message);
            }

        }

        //Alterar Imagem
        [Authorize(Roles ="user")]
        [HttpPatch("Alterar-Imagem-por-{id}")]
        public async Task<ActionResult<Produto>> AlterarImagem(int id, ImagemDto dto)
        {
            try
            {
                var produto = await _context.Produtos.FindAsync(id);
                // Tratamentos de erros

                if (produto == null) return NotFound("Produto não encontrado");
                if (dto.ImagemBase64 == null || dto.ImagemBase64.Length == 0) return BadRequest("Imagem não pode ser nula ou vazia");

                // Convertendo a imagem de base64 para byte[]
                produto.IMG_PRODUTO = Convert.FromBase64String(dto.ImagemBase64);
                _context.Produtos.Update(produto);
                await _context.SaveChangesAsync();
                return Ok(produto);

            }
            catch (Exception ex)
            {
                return BadRequest("Ocorreu um erro no sistema : " + ex.Message);
            }
        }

        //DELETs
        //Deletar Produto {id}
        [Authorize(Roles = "user,admin")]
        [HttpDelete("/deletar-produto-por-{id}")]
        public async Task<IActionResult> DeletarProduto(int id)
        {
            var id_produto = await _context.Produtos.FindAsync(id);
            if (id_produto == null) return NotFound("Produto não encontrado");

            _context.Produtos.Remove(id_produto);
            await _context.SaveChangesAsync();
            return Ok( new { mensagem = "Produto Removido com sucesso"});
        }
    }
}
