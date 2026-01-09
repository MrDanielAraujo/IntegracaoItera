using IntegracaoItera.Configuration;
using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace IntegracaoItera.Services;

public class TokenService : ITokenService
{
    private readonly IMemoryCache _cache;
    private readonly IJwtValidator _jwtValidator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServerSettings _settings;

    public TokenService(
        IMemoryCache cache,
        IJwtValidator jwtValidator,
        IHttpClientFactory httpClientFactory,
        IOptions<ServerSettings> settings)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _jwtValidator = jwtValidator ?? throw new ArgumentNullException(nameof(jwtValidator));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Tenta obter do cache
        if (_cache.TryGetValue(_settings.CacheKey, out string? cachedToken))
        {
            if (!_jwtValidator.IsTokenExpired(cachedToken))
            {
                return cachedToken;
            }
        }

        // Solicita novo token
        var accessToken = await RequestTokenAsync("IteraAuth", cancellationToken);

        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(55)); // Token geralmente expira em 60 min

        _cache.Set(_settings.CacheKey, accessToken.Token, cacheOptions);

        return accessToken.Token;
    }


    /// <summary>
    /// Realiza a requisição de token ao endpoint de autenticação.
    /// </summary>
    private async Task<AccessTokenDto> RequestTokenAsync(string client = "IteraAuth", CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(client);

        var authPayload = new
        {
            username = _settings.Auth.Username,
            password = _settings.Auth.Password
        };

        var content = new StringContent(JsonSerializer.Serialize(authPayload), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(_settings.Endpoints.Auth, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<AccessTokenDto>(responseContent) ?? new AccessTokenDto();
    }
}
