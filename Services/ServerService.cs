using IntegracaoItera.Interfaces;

namespace IntegracaoItera.Services;

public class ServerService(IApiServer apiServer, IClientService clientService) : IServerService
{

    private readonly IApiServer _apiServer = apiServer ?? throw new ArgumentNullException(nameof(apiServer));
    private readonly IClientService _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));

    public Task<bool> CheckStatusAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GetResultAsync(CancellationToken cancellationToken = default)
    {
        // vai verificar se o conteudo ja foi processado. se conteudo processado, chamar o sendResulAssync.

        var cnpj = string.Empty;

        _clientService.SendResultAsync(cnpj);

        throw new NotImplementedException();
    }

    public Task<string> SendContentAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
