using IntegracaoItera.Data.DTOs;

namespace IntegracaoItera.Interfaces;

public interface IClientService
{
    
    Task<ClientResponseDto> SendResultAsync(string cnpj, CancellationToken cancellationToken = default);

    Task<bool> SetContentAsync(ClientRequestDto request, CancellationToken cancellationToken = default);
}

