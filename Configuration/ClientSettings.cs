using IntegracaoItera.Interfaces;

namespace IntegracaoItera.Configuration;

public class ClientSettings : IClientServerSettings
{
    public const string SettingName = "ClientSettings";
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
    public EndpointClientSettings Endpoints { get; set; } = new();
    
    IEndpointSettings IClientServerSettings.Endpoints { get => Endpoints; set => throw new NotImplementedException(); }
}

/// <summary>
/// Configurações de endpoints da API Itera.
/// </summary>
public class EndpointClientSettings : IEndpointSettings
{
    /// <summary>
    /// URL do endpoint de autenticação
    /// </summary>
    public string Auth { get; set; } = string.Empty;

    /// <summary>
    /// URL do endpoint de upload de documentos
    /// </summary>
    public string Result { get; set; } = string.Empty;
}