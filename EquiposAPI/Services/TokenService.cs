using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EquiposAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace EquiposAPI.Services;

public interface ITokenService
{
    string GenerateToken(Usuario usuario);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Usuario usuario)
    {
        var jwtKey = _configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("La clave JWT no está configurada.");

        if (jwtKey.Length < 32)
        {
            throw new InvalidOperationException("La clave JWT debe tener al menos 32 caracteres por seguridad.");
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "EquiposAPI";
        var audience = _configuration["Jwt:Audience"] ?? "EquiposClients";
        var expireMinutesStr = _configuration["Jwt:ExpireMinutes"] ?? "120";
        if (!double.TryParse(expireMinutesStr, out var expireMinutes))
        {
            expireMinutes = 120;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Username),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
