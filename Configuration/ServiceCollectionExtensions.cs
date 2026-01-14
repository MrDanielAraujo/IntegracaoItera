using IntegracaoItera.Data;
using IntegracaoItera.Interfaces;
using IntegracaoItera.Services;
using Microsoft.EntityFrameworkCore;

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
        services.Configure<ServerSettings>(configuration.GetSection(ServerSettings.SettingName));
        services.Configure<ClientSettings>(configuration.GetSection(ClientSettings.SettingName));

        // Registra HttpClient usando IHttpClientFactory (melhor prática para gerenciamento de conexões)
        // services.AddHttpClient("IteraAuth");
        // services.AddHttpClient("IteraApi");

        services.AddHttpClient(ServerSettings.SettingName);
        services.AddHttpClient(ClientSettings.SettingName);

        // Registra serviços de cache
        services.AddMemoryCache();

        // ===== Serviços da versão anterior (compatibilidade) =====

        // Singleton: IJwtValidator - não possui estado mutável
        services.AddSingleton<IJwtValidator, JwtValidatorService>();

        // Scoped: Serviços que podem ter estado por requisição
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthorizedHttpClientFactory, AuthorizedHttpClientService>();
        services.AddScoped<IApiServer, ApiServerService>();
        services.AddScoped<IApiClient, ApiClientService>();
       
        services.AddScoped<IClientServerService, ClientServerService>();
        services.AddScoped<IAnoDocumentoService, AnoDocumentoService>();
        services.AddScoped<IDocumentoValidadorService, DocumentoValidadorService>();
        services.AddScoped<IDocumentoRepository, DocumentoService>();

        services.AddHostedService<DocumentoStatusWorkerService>();

        services.Configure<DocumentPollingOptions>(configuration.GetSection("DocumentPolling"));

        // Registra serviço de processamento de documentos (versão anterior)
        // services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

        // ===== Novos serviços (especificação IntraCred) =====

        // Serviço de validação
        // services.AddScoped<IDocumentoIntraCredValidationService, DocumentoIntraCredValidationService>();

        // Serviço de processamento
        // services.AddScoped<IDocumentoIntraCredProcessingService, DocumentoIntraCredProcessingService>();

        return services;
    }

    /// <summary>
    /// Adiciona o contexto do banco de dados e repositórios.
    /// Configurado para usar banco em memória, mas preparado para migração para PostgreSQL.
    /// </summary>
    /// <param name=services>Coleção de serviços</param>
    /// <param name=configuration>Configuração da aplicação</param>
    /// <returns>Coleção de serviços configurada</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuração do banco de dados
        // Para usar PostgreSQL, substituir por:
        services.AddDbContext<IntegraDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // services.AddDbContext<IteraDbContext>(options =>
        //    options.UseInMemoryDatabase(IteraClientDb));

        // Registra repositórios
        services.AddScoped<IDocumentoRepository, DocumentoService>();
        //services.AddScoped<IDocumentRepository, DocumentRepository>();
        //services.AddScoped<IDocumentExportResultRepository, DocumentExportResultRepository>();

        return services;
    }

}
