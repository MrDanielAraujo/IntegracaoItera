using IntegracaoItera.Configuration;
using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.Extensions.Options;

namespace IntegracaoItera.Services;

public class ApiClientService(
    IAuthorizedHttpClientFactory httpClientFactory,
    IOptions<ClientSettings> settings) : IApiClient
{
    private readonly IAuthorizedHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ClientSettings _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

    public async Task<string> SendResultAsync(ClientResponseDto request, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(_settings, cancellationToken);

        using var formData = new MultipartFormDataContent
        {
            { new StringContent(request.Cnpj ?? string.Empty), "cnpj" },
            { new StringContent(request.Result ?? string.Empty), "result" }
        };
        
        var response = await httpClient.PostAsync(_settings.Endpoints.Result, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
