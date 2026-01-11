using IntegracaoItera.Configuration;
using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IntegracaoItera.Services;

public class ApiServerService(
    IAuthorizedHttpClientFactory httpClientFactory,
    IOptions<ServerSettings> settings,
    ITokenService tokenService) : IApiServer
{
    private readonly IAuthorizedHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ServerSettings _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

    public async Task<string> GetAccessToken(CancellationToken cancellationToken = default)
    {
        var token = await _tokenService.GetAccessTokenAsync(_settings, cancellationToken);

        return token!;
    }

    public async Task<string> GetDeParaAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync( _settings , cancellationToken);

        var endpoint = (EndpointSettings)_settings.Endpoints;

        var url = string.Format(endpoint.DePara, documentId);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<List<ServerExportJsonDto>> GetExportJsonAsync(long cnpj, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(_settings, cancellationToken);

        var endpoint = (EndpointSettings)_settings.Endpoints;

        var url = string.Format(endpoint.ExportJson, cnpj);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonNode = JsonNode.Parse(content);

        if (jsonNode is not JsonObject listasContainer)
        {
            return [];
        }

        var listaControladora = new List<ServerExportJsonDto>();

        foreach (var prop in listasContainer)
        {
            if (prop.Value is JsonArray jsonArray)
            {
                var items = JsonSerializer.Deserialize<List<ServerExportJsonDto>>(jsonArray.ToJsonString());
                if (items != null)
                {
                    listaControladora.AddRange(items);
                }
            }
        }

        return listaControladora;
    }

    public async Task<ServerStatusResponseDto> GetStatusAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(_settings, cancellationToken);

        var endpoint = (EndpointSettings)_settings.Endpoints;

        var url = string.Format(endpoint.Status, documentId);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ServerStatusResponseDto>(content) ?? new ServerStatusResponseDto();
    }

    public async Task<string> UploadDocumentAsync(IFormFile file, string source, string description, string cnpj, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("O arquivo é obrigatório e não pode estar vazio.", nameof(file));
        }

        if (string.IsNullOrEmpty(cnpj))
        {
            throw new ArgumentException("O Cnpj é uma informação obrigatoria.", nameof(file));
        }

        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(_settings, cancellationToken);

        using var formData = new MultipartFormDataContent();

        // Adiciona o arquivo - copia para MemoryStream para evitar problemas com stream fechado
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var streamContent = new StreamContent(memoryStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
        formData.Add(streamContent, "file", file.FileName);

        // Adiciona os campos de texto
        formData.Add(new StringContent(source ?? string.Empty), "source");
        formData.Add(new StringContent(description ?? string.Empty), "description");
        formData.Add(new StringContent(cnpj ?? string.Empty), "cnpj");

        var endpoint = (EndpointSettings)_settings.Endpoints;

        var response = await httpClient.PostAsync(endpoint.UploadDoc, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
