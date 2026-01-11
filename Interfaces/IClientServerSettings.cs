using IntegracaoItera.Configuration;

namespace IntegracaoItera.Interfaces;

public interface IClientServerSettings
{
    /// <summary>
    /// Nome da seção no appsettings.json
    /// </summary>
    public string SectionName { get; }

    /// <summary>
    /// Configurações de autenticação
    /// </summary>
    public AuthSetings Auth { get; set; }

    /// <summary>
    /// Configurações de endpoints da API
    /// </summary>
    public IEndpointSettings Endpoints { get; set; }

}
