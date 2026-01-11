using IntegracaoItera.Data.DTOs;

namespace IntegracaoItera.Interfaces;

public interface IApiClient
{
    /// <summary>
    /// Obtém o status de um documento pelo ID.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status do documento</returns>
    Task<string> SendResultAsync(ClientResponseDto request, CancellationToken cancellationToken = default);
}
