using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TesouroAzulAPI.Data;
using TesouroAzulAPI.Dtos;
using TesouroAzulAPI.Models;
using TesouroAzulAPI.Services;

namespace TesouroAzulAPI.Controllers
{

    [Route("api/Usuarios"), ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Instanciando o context da ApplicationDbContext para _context
        public UsuarioController(ApplicationDbContext context) { _context = context; }

        // DTO (Data Transfer Object) para o Http AlterarCamposUsuario 
        public class AtualizarCampoUsuarioDto 
        {
            public string Campo { get; set; } = string.Empty;
            public string NovoValor { get; set; } = string.Empty;

        }


        // POSTs
        // Criar Usuario
        [HttpPost("criar-usuario")]
        public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioDto usuarioDto) 
        {
            if (await _context.Usuarios.AnyAsync(u => u.EMAIL_USUARIO == usuarioDto.EMAIL_USUARIO|| u.CPF_USUARIO == usuarioDto.CPF_USUARIO))
            {
                return BadRequest(new {mensagem = "Email ou CPF já cadastrado." });
            }

            var usuario = new Usuario
            {
                NOME_USUARIO = usuarioDto.NOME_USUARIO,
                EMAIL_USUARIO = usuarioDto.EMAIL_USUARIO,
                DATA_NASC_USUARIO = usuarioDto.DATA_NASC_USUARIO,
                CPF_USUARIO = usuarioDto.CPF_USUARIO,
                CNPJ_USUARIO = usuarioDto.CNPJ_USUARIO,
                ID_ASSINATURA_FK = usuarioDto.ID_ASSINATURA_FK,
                FOTO_USUARIO = String.IsNullOrEmpty(usuarioDto.FOTO_USUARIO) ? null : Convert.FromBase64String(usuarioDto.FOTO_USUARIO),
                SENHA_USUARIO = usuarioDto.SENHA_USUARIO,
                STATUS_USUARIO = "a"

            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(BuscarUsuarioPorId), new { id = usuario.ID_USUARIO }, usuario);
        }

        // Realizar login 
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, [FromServices] TokenService tokenService)
        {
            // tratamentos de erros

            if (string.IsNullOrEmpty(dto.EMAIL_USUARIO) || string.IsNullOrEmpty(dto.SENHA_USUARIO)) { return BadRequest("Email e Senha são obrigatórios."); }
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.EMAIL_USUARIO == dto.EMAIL_USUARIO && u.SENHA_USUARIO == dto.SENHA_USUARIO);

            if (usuario == null) { return Unauthorized("Email ou senhas invalidos"); }

            // Verifica se o usuario está ativo

            if (usuario.STATUS_USUARIO != "a") { return BadRequest("Usuario inativo. Entre em contato com o suporte."); }

            // Gera token JWT
            var token = tokenService.GenerateTokenUser(usuario);

            // Retorna Usuario

            return Ok(new
            {
                mensagem = "Login realizado com sucesso.",
                token
            });
        }

        // GETs
        // Endpoint para Admin
        // Buscar Usuarios
        [Authorize(Roles = "admin")] // remover user no futuro
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> BuscarUsuarios() 
        {
            return await _context.Usuarios.ToListAsync();
        }

        // Endpint para Usuario
        // Buscar Usuario por ID
        [Authorize(Roles = "user,admin")]
        [HttpGet("/buscar-dados-usuario")]
        public async Task<ActionResult<Usuario>> BuscarUsuarioPorId() 
        {
            var usuario = await _context.Usuarios.FindAsync(Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value));
            if (usuario == null) return NotFound("Usuario não encontrado.");
            return usuario;
        }

        // Buscar Imagem por ID
        [Authorize(Roles = "user,admin")]
        [HttpGet("Buscar-Imagem")]
        public async Task<ActionResult<Usuario>> BuscarUsuarioFoto()
        {
            var usuario = await _context.Usuarios.FindAsync(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));

            // Tratamento de erro quando não encontrar o usuario

            if (usuario == null) return NotFound("Usuario não encontrado.");

            if (usuario.FOTO_USUARIO == null || usuario.FOTO_USUARIO.Length == 0) { return NotFound("Usuario não possui imagem."); }

            // Converte a imagem para Base64
            var imagemBase64 = Convert.ToBase64String(usuario.FOTO_USUARIO);
            return Ok(new { mensagem = "Imagem encontrada com sucesso.", imagemBase64 });
        }

        // PATCHs
        // Atualizar Usuario
        [Authorize(Roles = "user,admin")]
        [HttpPatch("/alterar-campo")]
        public async Task<IActionResult> AlterarCamposUsuario(AtualizarCampoUsuarioDto dto)
        {
            // Controller Dinâmico, ou seja, utiliza a classe AtualizarCampoUsuarioDto para qualquer campo e informação que deseja alterar

            // Tratamentos de erro de {id}
            // Criar uma função privada de mensagem de erro mais tarde aqui para ecomonizar codigo
            var usuario = await _context.Usuarios.FindAsync(Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value));

            if (usuario == null) return NotFound("Usuario não encontrado.");

            if (string.IsNullOrEmpty(dto.Campo) || string.IsNullOrEmpty(dto.NovoValor))
            {
                return BadRequest("Campo ou Novo Valor não podem ser nulos ou vazios.É permitido os seguintes campos : 'nome', 'email', 'cpf', 'cnpj', 'senha', 'data'");
            }

            switch (dto.Campo)
            {
                case "nome":
                    usuario.NOME_USUARIO = dto.NovoValor;
                    break;
                case "email":
                    usuario.EMAIL_USUARIO = dto.NovoValor;
                    break;
                case "cpf":
                    usuario.CPF_USUARIO = dto.NovoValor;
                    break;
                case "cnpj":
                    usuario.CNPJ_USUARIO = dto.NovoValor;
                    break;
                case "senha":
                    usuario.SENHA_USUARIO = dto.NovoValor;
                    break;
                case "data":
                    if (DateTime.TryParse(dto.NovoValor, out DateTime dataNasc))
                    {
                        usuario.DATA_NASC_USUARIO = dataNasc;
                    }
                    else
                    {
                        return BadRequest("Novo Valor para Data deve ser uma data válida.");
                    }
                    break;
                default:
                    return BadRequest("Campo inválido. É permitido os seguintes campos : 'nome', 'email', 'cpf', 'cnpj', 'senha', 'data'");
            }

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Campo Atualizado com sucesso.", usuario });
        }

        // Desativar Usuario
        [Authorize(Roles = "user,admin")]
        [HttpPatch("/desativar-usuario")]
        public async Task<IActionResult> DesativarUsuario()
        {
            // Busca pelo id
            var usuario = await _context.Usuarios.FindAsync(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (usuario == null) return NotFound("Usuario não encontrado.");
            // Verifica se o usuario já está desativado
            if (usuario.STATUS_USUARIO == "d") return BadRequest("Usuario já está desativado.");
            usuario.STATUS_USUARIO = "d"; // Define o status como desativado
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Usuario desativado com sucesso.", usuario });
        }

        // Reativar Usuario
        [Authorize(Roles = "user")]
        [HttpPatch("/reativar-usuario")]
        public async Task<IActionResult> ReativarUsuario()
        {
            // Busca pelo id
            var usuario = await _context.Usuarios.FindAsync(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (usuario == null) return NotFound("Usuario não encontrado.");
            // Verifica se o usuario já está ativo
            if (usuario.STATUS_USUARIO == "a") return BadRequest("Usuario já está ativo.");
            usuario.STATUS_USUARIO = "a"; // Define o status como ativo
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Usuario reativado com sucesso.", usuario });
        }

        // Ativar assinatura
        [Authorize(Roles = "user,admin")]
        [HttpPatch("/ativar-assinatura")]
        public async Task<IActionResult> AtivarAssinatura()
        {
            var usuario = await _context.Usuarios.FindAsync(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (usuario == null) return NotFound("Usuario não encontrado.");

            // Verifica se o usuario já é assinante
            if (usuario.ID_ASSINATURA_FK == 2) return BadRequest("Usuario já é assinante.");

            usuario.ID_ASSINATURA_FK = 2; // Define o status como assinante

            // Atualiza a data validade com duração de 6 meses após o pagamento
            DateTime dataValidade = DateTime.Now.AddMonths(6);

            usuario.DATA_INICIO_ASSINATURA_USUARIO = DateTime.Now;
            usuario.DATA_VALIDADE_ASSINATURA_USUARIO = dataValidade;


            // Salva contexto
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Assinatura ativada com sucesso.", usuario });
        }

            // Atualizar Imagem
            [Authorize(Roles = "user,admin")]
        [HttpPatch("/alterar-imagem")]
        public async Task<IActionResult> AtualizarImagem([FromBody] ImagemDto dto)
        {
            // Busca pelo id
            var usuario = await _context.Usuarios.FindAsync(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (usuario == null) return NotFound("Usuario não encontrado.");

            try
            {
                byte[] imagemBytes = Convert.FromBase64String(dto.ImagemBase64);

                // Tratamento de erro quando receber o JSON
                dto.ImagemBase64 = dto.ImagemBase64.Replace("data:image/png;base64,", string.Empty);
                dto.ImagemBase64 = dto.ImagemBase64.Replace("\r", "").Replace("\n", "");
                // Tratamento para tamnhos de imagem
                if (imagemBytes.Length > 16777215) return BadRequest("Imagem muito grande. O tamanho máximo é de 16MB.");

                usuario.FOTO_USUARIO = imagemBytes;

                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
                return Ok(new { mensagem = "Imagem Atualizada com sucesso.", usuario });
        }
            catch (FormatException) 
            {
                return BadRequest("Formatação de imagem64 incorreta");
            }


        }

        // DELETEs
        // EndPoint para Admin
        // Deletar Usuario
        [Authorize(Roles = "user,admin")] // Temporario para usuario
        [HttpDelete("{id}/deletar-usuario")]
        public async Task<IActionResult> DeletarUsuario(int id) 
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound("Usuario não encontrado."); // Tratamento de erro

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
