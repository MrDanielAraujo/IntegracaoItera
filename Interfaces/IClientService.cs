using IntegracaoItera.Data.DTOs;

namespace IntegracaoItera.Interfaces;

public interface IClientService
{
    
    Task<ClientResponseDto> SendResultAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task<bool> SetContentAsync(ClientRequestDto request, CancellationToken cancellationToken = default);
}

