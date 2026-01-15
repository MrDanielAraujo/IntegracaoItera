using IntegracaoItera.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace IntegracaoItera.Services;

[AllowAnonymous]
public class JwtValidatorService : IJwtValidator
{
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtValidatorService()
    {
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public bool IsTokenExpired(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return true;

        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            // Se não conseguir ler o token, considera como expirado
            return true;
        }
    }
}
