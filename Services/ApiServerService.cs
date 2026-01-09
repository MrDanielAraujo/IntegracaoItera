using IntegracaoItera.Configuration;
using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IntegracaoItera.Services;

public class ApiServerService : IApiServer
{
    private readonly IAuthorizedHttpClientFactory _httpClientFactory;
    private readonly ServerSettings _settings;

    public ApiServerService(
        IAuthorizedHttpClientFactory httpClientFactory,
        IOptions<ServerSettings> settings)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<string> GetDeParaAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(cancellationToken);

        var url = string.Format(_settings.Endpoints.DePara, documentId);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<List<ServerExportJsonDto>> GetExportJsonAsync(long cnpj, CancellationToken cancellationToken = default)
    {
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(cancellationToken);

        var url = string.Format(_settings.Endpoints.ExportJson, cnpj);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonNode = JsonNode.Parse(content);

        if (jsonNode is not JsonObject listasContainer)
        {
            return new List<ServerExportJsonDto>();
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
        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(cancellationToken);

        var url = string.Format(_settings.Endpoints.Status, documentId);

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

        using var httpClient = await _httpClientFactory.CreateAuthorizedClientAsync(cancellationToken);

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

        var response = await httpClient.PostAsync(_settings.Endpoints.UploadDoc, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
