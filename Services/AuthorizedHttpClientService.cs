using IntegracaoItera.Interfaces;
using System.Net.Http.Headers;

namespace IntegracaoItera.Services;

public class AuthorizedHttpClientService : IAuthorizedHttpClientFactory
{
    private readonly ITokenService _tokenService;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthorizedHttpClientService(
        ITokenService tokenService,
        IHttpClientFactory httpClientFactory)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<HttpClient> CreateAuthorizedClientAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = await _tokenService.GetAccessTokenAsync(cancellationToken);
        var client = _httpClientFactory.CreateClient("IteraApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
