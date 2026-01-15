using IntegracaoItera.Common;
using IntegracaoItera.Data;
using IntegracaoItera.Data.Enums;
using IntegracaoItera.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace IntegracaoItera.Middleware;

public sealed class UserExistsHandler : AuthorizationHandler<UserExistsRequirement>
{
    private readonly IntegraDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserExistsHandler(IntegraDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserExistsRequirement requirement)
    {

        var token = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(token) && token.StartsWith("Bearer "))
        {
            token = token.Replace("Bearer ", "").Trim();
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var payload = jwtToken.Payload.ToDictionary(k => k.Key, v => v.Value);

        if (context.User?.Identity?.IsAuthenticated == false)
        {
            context.Fail();
            return;
        }

        try
        {
            // Verificando se o token é do issuer intrabank (interno, portanto não deve acessar rotas internas)
            var issuer = context.User!.Claims.FirstOrDefault(x => x.Type == "iss")!.Value.ToUpperInvariant();
            if (issuer.Equals("Intrabank.Auth", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Fail();
                return;
            }

            var name = context.User!.Claims.FirstOrDefault(x => x.Type == "name")!.Value.ToUpperInvariant();

            var email = payload.TryGetValue("unique_name", out var uniqueNameValue)
                ? uniqueNameValue?.ToString()
                : payload.TryGetValue("preferred_username", out var preferredUsernameValue)
                    ? preferredUsernameValue?.ToString()
                    : null;

            var userId = (string?)jwtToken.Payload.ToDictionary(k => k.Key, v => v.Value)["oid"];

            var roles = (JsonElement)jwtToken.Payload.ToDictionary(k => k.Key, v => v.Value)["roles"];
            var rolesList = roles.EnumerateArray().Select(s => s.GetString()).Where(x => !x.IsNullOrEmptyOrZero()).ToList();
            var userGuid = Guid.Parse(userId!);

            if (userGuid == Guid.Empty)
            {
                context.Fail(); // autenticado inválido
                return;
            }

            // Verifica se existe no seu banco (ajuste a entidade/coluna)
            var usuarioExiste = await _db.Usuarios.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(u => u.Id == userGuid);

            if (usuarioExiste is not null)
            {
                var setor = Enum.TryParse(rolesList.FirstOrDefault()!.Split('.')[1], true, out UsuarioSetor result) ? result : UsuarioSetor.Outros;
                var cargo = Enum.TryParse(rolesList.FirstOrDefault()!.Split('.')[2], true, out TipoCargo result1) ? result1 : TipoCargo.Outros;

                usuarioExiste.Setor = setor;
                usuarioExiste.Cargo = cargo;

                _db.Usuarios.Update(usuarioExiste);
                await _db.SaveChangesAsync();

                context.Succeed(requirement);
            }
            else
            {
                var setor = Enum.TryParse(rolesList.FirstOrDefault()!.Split('.')[1], true, out UsuarioSetor result) ? result : UsuarioSetor.Outros;
                var cargo = Enum.TryParse(rolesList.FirstOrDefault()!.Split('.')[2], true, out TipoCargo result1) ? result1 : TipoCargo.Outros;

                var novoUsuario = new Usuario
                {
                    Id = userGuid,
                    Cargo = cargo,
                    Setor = setor,
                    Email = email,
                    Nome = name,
                    DataCriacao = DateTime.UtcNow,
                };

                await _db.Usuarios.AddAsync(novoUsuario);
                await _db.SaveChangesAsync();

                context.Succeed(requirement);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex.InnerException);
        }
    }
}
