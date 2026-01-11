using IntegracaoItera.Interfaces;
using System.Net.Http.Headers;

namespace IntegracaoItera.Services;

public class AuthorizedHttpClientService(
    ITokenService tokenService,
    IHttpClientFactory httpClientFactory) : IAuthorizedHttpClientFactory
{
    private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    public async Task<HttpClient> CreateAuthorizedClientAsync(IClientServerSettings settings, CancellationToken cancellationToken = default)
    {
        var accessToken = await _tokenService.GetAccessTokenAsync(settings, cancellationToken);
        var httpClient = _httpClientFactory.CreateClient(settings.SectionName);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return httpClient;
    }
}
