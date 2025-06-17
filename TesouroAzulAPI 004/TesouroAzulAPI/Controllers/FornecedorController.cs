using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Dtos;
using TesouroAzulAPI.Models;

namespace TesouroAzulAPI.Controllers
{
    [Route("api/Fornecedores"), ApiController]
    public class FornecedorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Instanciando o context da ApplicationDbContext para _context
        public FornecedorController(ApplicationDbContext context) { _context = context; }

        // DTO para Http AlterarCamposFornecedor
        public class AtualizarCampoFornecedorDto
        {
            public string Campo { get; set; } = string.Empty;
            public string NovoValor { get; set; } = string.Empty;
        }


        //POSTs
        //Criar Fornecedor
        [Authorize(Roles = "user")]
        [HttpPost("criar-fornecedor")]
        public async Task<IActionResult> CriarFornecedor([FromBody] CriarFornecedorDto dto)
        {
            // Busca o ID do usuário logado
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // Tratamento de erro
            if (await _context.Fornecedores.AnyAsync(f => f.CNPJ_FORNECEDOR == dto.CNPJ_FORNECEDOR && idUsuario == f.ID_USUARIO_FK)) { return BadRequest(new { mensagem = "CNPJ já cadastrado." }); }

            var fornecedor = new Fornecedor
            {
                ID_USUARIO_FK = idUsuario,
                NOME_FORNECEDOR = dto.NOME_FORNECEDOR,
                CNPJ_FORNECEDOR = dto.CNPJ_FORNECEDOR,
                EMAIL_FORNECEDOR = dto.EMAIL_FORNECEDOR,
                TEL_FORNECEDOR = dto.TELEFONE_FORNECEDOR ?? null,
                CEL_FORNECEDOR = dto.CEL_FORNECEDOR,
                ENDERECO_FORNECEDOR = dto.ENDERECO_FORNECEDOR
            };

            _context.Fornecedores.Add(fornecedor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(BuscarFornecedorPorId), new { id = fornecedor.ID_FORNECEDOR }, fornecedor);
        }

        // Buscar fornecedor por semelhancia
        [Authorize(Roles = "user")]
        [HttpPost("buscar-fornecedor-por-nome-similar")]
        public async Task<IActionResult> BuscarFornecedorPorSimiliar([FromBody] string nomeFornecedor)
        {
            if (string.IsNullOrEmpty(nomeFornecedor)) return BadRequest("O nome do fornecedor não pode ser nulo ou vazio.");
            
            // Busca o ID do usuário logado
            int idUsuario = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            // Busca fornecedores que contenham o nome fornecido e pertencem ao usuário logado
            var fornecedoresEncontrados = await _context.Fornecedores
                .Where(f => f.NOME_FORNECEDOR.Contains(nomeFornecedor) && f.ID_USUARIO_FK == idUsuario)
                .ToListAsync();
            return Ok(fornecedoresEncontrados);
        }
        //GETs
        //Buscar Fornecedores
        [Authorize(Roles = "admin")] 
        [HttpGet("buscar-fornecedores")]
        public async Task<ActionResult<IEnumerable<Fornecedor>>> BuscarFornecedores()
        {
            return await _context.Fornecedores.ToListAsync();
        }

        //Burcar Forncecedor por ID
        [Authorize(Roles = "user")]
        [HttpGet("buscar-fornecedor")]
        public async Task<ActionResult<Fornecedor>> BuscarFornecedorPorId()
        {
            // Buscar o ID do usuário logado
            int id = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null) return NotFound(new { mensagem = "Fornecedor não encontrado." });
            return fornecedor;
        }

        //PACHTs
        //Alterar Fornecedor
        [Authorize(Roles = "user,admin")]
        [HttpPatch("alterar-fornecedor-por-campo")]
        public async Task<IActionResult> AlterarFornecedor([FromBody] AtualizarCampoFornecedorDto dto)
        {
            // Buscar o ID do usuário logado
            int id = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var fornecedor = await _context.Fornecedores.FindAsync(id);
            if (fornecedor == null) return NotFound(new { mensagem = "Fornecedor não encontrado." });

            if (string.IsNullOrEmpty(dto.Campo) || string.IsNullOrEmpty(dto.NovoValor)) return BadRequest("Campo ou Novo Valor não podem ser nulos ou vazios.É permitido os seguintes campos : 'nome', 'cnpj', 'email', 'telefone', 'celular', 'endereco'");

            // Atribuindo novo valor ao campo escolhido
            switch (dto.Campo.ToLower())
            {
                case "nome":
                    fornecedor.NOME_FORNECEDOR = dto.NovoValor;
                    break;
                case "cnpj":
                    if (await _context.Fornecedores.AnyAsync(f => f.CNPJ_FORNECEDOR == dto.NovoValor)) return BadRequest(new { mensagem = "CNPJ já cadastrado." });
                    fornecedor.CNPJ_FORNECEDOR = dto.NovoValor;
                    break;
                case "email":
                    if (await _context.Fornecedores.AnyAsync(f => f.EMAIL_FORNECEDOR == dto.NovoValor)) return BadRequest(new { mensagem = "Email já cadastrado." });
                    fornecedor.EMAIL_FORNECEDOR = dto.NovoValor;
                    break;
                case "telefone":
                    if (await _context.Fornecedores.AnyAsync(f => f.TEL_FORNECEDOR == dto.NovoValor)) return BadRequest(new { mensagem = "Telefone já cadastrado." });
                    fornecedor.TEL_FORNECEDOR = dto.NovoValor;
                    break;
                case "celular":
                    if (await _context.Fornecedores.AnyAsync(f => f.CEL_FORNECEDOR == dto.NovoValor)) return BadRequest(new { mensagem = "Celular já cadastrado." });
                    fornecedor.CEL_FORNECEDOR = dto.NovoValor;
                    break;
                case "endereco":
                    fornecedor.ENDERECO_FORNECEDOR = dto.NovoValor;
                    break;
                default:
                    return BadRequest("Campo inválido. Os campos válidos são: 'nome', 'cnpj', 'email', 'telefone', 'celular', 'endereco'");
            }

            _context.Fornecedores.Update(fornecedor);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Fornecedor atualizado com sucesso." });

        }


        //DELETEs
        //Deletar Fornecedor
        [Authorize(Roles = "user,admin")]
        [HttpDelete("{id_fornecedor}")]
        public async Task<IActionResult> DeletarFornecedor(int id_fornecedor)
        {
            var fornecedor = await _context.Fornecedores.FindAsync(id_fornecedor);
            if (fornecedor == null) return NotFound(new { mensagem = "Fornecedor não encontrado." });

            _context.Fornecedores.Remove(fornecedor);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Fornecedor deletado com sucesso." });
        }
    }
}
