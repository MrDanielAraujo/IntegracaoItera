using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;

namespace IntegracaoItera.Services;

public class ClientService(IApiClient apiClient, IServerService serverService) : IClientService
{

    private readonly IApiClient _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    private readonly IServerService _serverService = serverService ?? throw new ArgumentNullException(nameof(serverService));

    public async Task<ClientResponseDto> SendResultAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        // 1) Buscar o registro no banco de dados atravez do cnpj, obeter sempre o ultivo registro enviado.
        // 1.1) varificar se o status campo ServerStatus esta com o status de successo. caso não esteja cancelar. 
        // 2) Enviar o campo ServerResult e ClientCnpj

        var result = await _apiClient.SetResultAsync(new ClientResponseDto(), cancellationToken);
        
        // 3) Atualizar o registro campo ClientStatus como Devolvido.

        throw new NotImplementedException();
    }

    public Task<bool> SetContentAsync(ClientRequestDto request, CancellationToken cancellationToken = default)
    {
        // 1) Validar os Dados.
        // 1.1) Filtrar os dados.
        // 2) Incluir registro no banco de dados com o campo ClientStatus = Recebido
        
        // 3) chamar o server para enviar os dados. ServerService.SendContentAsync
        _serverService.SendContentAsync();

        throw new NotImplementedException();
    }

}

