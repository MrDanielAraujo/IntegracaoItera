using IntegracaoItera.Configuration;
using IntegracaoItera.Interfaces;
using Microsoft.Extensions.Options;

namespace IntegracaoItera.Services;

public class DocumentoStatusWorkerService(IServiceScopeFactory scopeFactory, IOptions<DocumentPollingOptions> options) : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly DocumentPollingOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var documentoService = scope.ServiceProvider.GetService<IDocumentoRepository>();
            var clientServerService = scope.ServiceProvider.GetRequiredService<IClientServerService>();


            Console.WriteLine("Estou inciado Pombetas!");

            var documents = await documentoService.ObterPorStatusjAsync(stoppingToken);

            if (documents.Count == 0)
            {
                await Task.Delay( TimeSpan.FromMinutes(_options.IdleDelayMinutes), stoppingToken);

                continue;
            }

            foreach (var document in documents)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                await clientServerService.ServerCheckStatusAsync(document, stoppingToken);
            }

            // ⚠️ Importante:
            // Se processou documentos, NÃO espera.
            // Volta imediatamente ao banco.
        }
    }
}
