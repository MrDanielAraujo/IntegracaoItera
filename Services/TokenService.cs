using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace IntegracaoItera.Services;

[AllowAnonymous]
public class TokenService(
    IMemoryCache cache,
    IJwtValidator jwtValidator,
    IHttpClientFactory httpClientFactory) : ITokenService
{
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    private readonly IJwtValidator _jwtValidator = jwtValidator ?? throw new ArgumentNullException(nameof(jwtValidator));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    public async Task<string?> GetAccessTokenAsync(IClientServerSettings settings, CancellationToken cancellationToken = default)
    {
        // Tenta obter do cache
        if (_cache.TryGetValue(settings.SectionName, out string? cachedToken))
        {
            if (!_jwtValidator.IsTokenExpired(cachedToken))
            {
                return cachedToken;
            }
        }

        // Solicita novo token
        var accessToken = await RequestTokenAsync(settings, cancellationToken);

        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(55)); // Token geralmente expira em 60 min

        _cache.Set(settings.SectionName, accessToken.Token, cacheOptions);

        return accessToken.Token;
    }


    /// <summary>
    /// Realiza a requisição de token ao endpoint de autenticação.
    /// </summary>
    private async Task<AccessTokenDto> RequestTokenAsync(IClientServerSettings settings, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(settings.SectionName);

        var authPayload = new
        {
            username = settings.Auth.Username,
            password = settings.Auth.Password,
        };

        var content = new StringContent(JsonSerializer.Serialize(authPayload), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(settings.Endpoints.Auth, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<AccessTokenDto>(responseContent) ?? new AccessTokenDto();
    }

     
}
