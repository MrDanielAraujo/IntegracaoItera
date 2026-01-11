using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;

namespace IntegracaoItera.Services;

public class ClientService : IClientService
{
    public Task<ClientResponseDto> SendResultAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        // 1) Buscar o registro no banco de dados atravez do documentId
        // 1.1) varificar se o status campo ServerStatus esta com o status de successo. caso não esteja cancelar. 
        // 2) Enviar o campo ServerResult e ClientCnpj
        // 3) Atualizar o registro campo ClientStatus como Devolvido.
        throw new NotImplementedException();
    }

    public Task<bool> SetContentAsync(ClientRequestDto request, CancellationToken cancellationToken = default)
    {
        // 1) Validar os Dados.
        // 1.1) Filtrar os dados.
        // 2) Incluir registro no banco de dados com o campo ServerStatus = Recebido e ClientStatus = Recebido
        throw new NotImplementedException();
    }

}

