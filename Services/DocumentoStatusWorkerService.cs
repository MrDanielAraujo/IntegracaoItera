using IntegracaoItera.Configuration;
using IntegracaoItera.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IntegracaoItera.Services;

/// <summary>
/// Classe de checagem de status e atualização de conteudo processado.
/// </summary>
/// <param name="scopeFactory"></param>
/// <param name="options"></param>
public class DocumentoStatusWorkerService(ILogger<DocumentoStatusWorkerService> logger,IServiceScopeFactory scopeFactory, IOptions<DocumentPollingOptions> options) : BackgroundService
{
    private readonly ILogger<DocumentoStatusWorkerService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly DocumentPollingOptions _options = options.Value;

    /// <summary>
    /// Metodo que é executado em segundo plano. 
    /// Esse metodo é executado ao inicar a aplicação.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[INTEGRACAO] => INICIANDO O PROCESSO DE VERIFICAÇÃO DE STATUS DE DOCUMENTOS!");

        while (!stoppingToken.IsCancellationRequested)
        {
            //Obtem o scopo
            using var scope = _scopeFactory.CreateScope();

            // Cria a instancia dos services que vamos utilizar. 
            var documentoService = scope.ServiceProvider.GetService<IDocumentoRepository>();
            var clientServerService = scope.ServiceProvider.GetRequiredService<IClientServerService>();

            // Obtem a lista de de documento por status.
            var documents = await documentoService!.ObterPendentesProcessamentoAsync(stoppingToken);


            _logger.LogInformation($"[INTEGRACAO] => A QUANTIDADE DE DOCUMENTOS QUE VAMOS VERIDICAR {documents!.Count}!");

            //Lendo cada documento encontrado para checar o status.
            foreach (var document in documents)
            {
                // verifica se seve ser cancelado a leitura dos documentos.
                if (stoppingToken.IsCancellationRequested) break;

                _logger.LogInformation($"[INTEGRACAO] => VAMOS VERIFICAR AGORA O DOCUMENTO {document.Id}!");

                // Envia o dumento para ser checado no servidor se o status já teve alguma alteração.
                var result = await clientServerService.ServerCheckStatusAsync(document, stoppingToken);

                // imprime no console o Id do documento verificado e o status que retornou.
                if (result is not null) _logger.LogInformation($"[INTEGRACAO] => DOCUMENTO {document.Id} VERIFICADO, CÓDIGO DO STATUS: {result.CodigoStatus}, DESCRIÇÃO: {result.DescricaoStatus}"); 

                // Após toda a verificação, aguardar um tempo para obter o proximo documento.
                await Task.Delay(TimeSpan.FromSeconds(_options.IdleDelayMinutes), stoppingToken);
            }

            documents = await documentoService!.ObterProcessadosNaoEnviadosAsync(stoppingToken);
            

            //Lendo cada documento encontrado para checar o status.
            foreach (var document in documents)
            {
                var result = await clientServerService.ServerGetResultAsync(document, stoppingToken);

                // imprime no console o Id do documento verificado e o status que retornou.
                if (result is not null) _logger.LogInformation($"[INTEGRACAO] => DOCUMENTO {document.Id} VERIFICADO, CÓDIGO DO STATUS: {result.CodigoStatus}, DESCRIÇÃO: {result.DescricaoStatus}");

                // Após toda a verificação, aguardar um tempo para obter o proximo documento.
                await Task.Delay(TimeSpan.FromSeconds(_options.IdleDelayMinutes), stoppingToken);
            }
            
            _logger.LogInformation($"[INTEGRACAO] => AGUARDANDO {_options.IdleDelayMinutes} MINUTOS PARA A PRÓXIMA CHECAGEM!");

            //caso não tenha registros para verificar, aguarda um tempo para fazer uma nova consulta.
            await Task.Delay(TimeSpan.FromMinutes(_options.IdleDelayMinutes), stoppingToken);

            // ⚠️ Importante:
            // Se processou documentos, NÃO espera.
            // Volta imediatamente ao banco.
        }
    }
}
