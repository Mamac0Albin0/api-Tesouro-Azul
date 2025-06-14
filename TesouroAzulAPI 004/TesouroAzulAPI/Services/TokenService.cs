using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TesouroAzulAPI.Models;

namespace TesouroAzulAPI.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        // Instancia com o IConfiguration para acessar as configurações do appsettings.json
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateTokenUser(Usuario usuario)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.ID_USUARIO.ToString()),
                    new Claim(ClaimTypes.Email, usuario.EMAIL_USUARIO),
                    new Claim(ClaimTypes.Role, "user"),
                    new Claim(ClaimTypes.Role, usuario.ID_ASSINATURA_FK == 2 ? "Assinante" : "NaoAssinante"),
                }),

                Expires = DateTime.UtcNow.AddHours(5), // Define a expiração do token
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Quando criar o administrador no banco adicionar as claims corretas
        public string GenerateTokenAdmin(Usuario usuario)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, "admin"),
                }),

                Expires = DateTime.UtcNow.AddHours(5), // Define a expiração do token
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
