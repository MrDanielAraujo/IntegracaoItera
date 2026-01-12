using IntegracaoItera.Configuration;
using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace IntegracaoItera.Services;

public class ApiClientService(
    IAuthorizedHttpClientFactory httpClientFactory,
    IOptions<ClientSettings> settings) : IApiClient
{
    private readonly IAuthorizedHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ClientSettings _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

    public async Task<string> SetResultAsync(ClientResponseDto request, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(_settings, cancellationToken);

        string jsonPayload = JsonSerializer.Serialize(request);

        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(_settings.Endpoints.Result, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
