using IntegracaoItera.Interfaces;

namespace IntegracaoItera.Configuration;

public class ServerSettings : IClientServerSettings
{
    public const string SettingName = "ServerSettings";
    /// <summary>
    /// Nome da seção no appsettings.json
    /// </summary>
    public string SectionName { get => SettingName; } 

    /// <summary>
    /// Configurações de autenticação
    /// </summary>
    public AuthSetings Auth { get; set; } = new();

    /// <summary>
    /// Configurações de endpoints da API
    /// </summary>
    public IEndpointSettings Endpoints { get; set; } = new EndpointSettings();
}

/// <summary>
/// Configurações de endpoints da API Itera.
/// </summary>
public class EndpointSettings : IEndpointSettings
{
    /// <summary>
    /// URL do endpoint de autenticação
    /// </summary>
    public string Auth { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint de upload de documentos
    /// </summary>
    public string UploadDoc { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint de status do documento (com placeholder {0} para o ID)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint de exportação JSON (com placeholder {0} para o CNPJ)
    /// </summary>
    public string ExportJson { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint De-Para (com placeholder {0} para o ID)
    /// </summary>
    public string DePara { get; set; } = string.Empty;
}
