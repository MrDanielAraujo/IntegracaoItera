using IntegracaoItera.Interfaces;
using IntegracaoItera.Services;

namespace IntegracaoItera.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona todos os serviços do Itera ao container de injeção de dependência.
    /// </summary>
    /// <param name="services">Coleção de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuração tipada usando Options Pattern
        services.Configure<ServerSettings>(configuration.GetSection(ServerSettings.SectionName));

        // Registra HttpClient usando IHttpClientFactory (melhor prática para gerenciamento de conexões)
        services.AddHttpClient("IteraAuth");
        services.AddHttpClient("IteraApi");

        // Registra serviços de cache
        services.AddMemoryCache();

        // ===== Serviços da versão anterior (compatibilidade) =====

        // Singleton: IJwtValidator - não possui estado mutável
        services.AddSingleton<IJwtValidator, JwtValidatorService>();

        // Scoped: Serviços que podem ter estado por requisição
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthorizedHttpClientFactory, AuthorizedHttpClientService>();
        services.AddScoped<IApiServer, ApiServerService>();

        // Registra serviço de processamento de documentos (versão anterior)
        // services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

        // ===== Novos serviços (especificação IntraCred) =====

        // Serviço de validação
        // services.AddScoped<IDocumentoIntraCredValidationService, DocumentoIntraCredValidationService>();

        // Serviço de processamento
        // services.AddScoped<IDocumentoIntraCredProcessingService, DocumentoIntraCredProcessingService>();

        return services;
    }

}
