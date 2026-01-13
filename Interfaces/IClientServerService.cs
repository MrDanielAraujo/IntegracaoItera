using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Models;

namespace IntegracaoItera.Interfaces;

public interface IClientServerService
{
    Task<MensagemRetornoDto> ClientSendResultAsync(string cnpj, CancellationToken cancellationToken = default);
   
    Task<MensagemRetornoDto> ClientSetContentAsync(ClientRequestDto request, CancellationToken cancellationToken = default);
    
    Task<MensagemRetornoDto> ServerCheckStatusAsync(Documento documento, CancellationToken cancellationToken = default);
    

    Task<MensagemRetornoDto> ServerGetResultAsync(Documento documento, CancellationToken cancellationToken = default);


    Task<MensagemRetornoDto> ServerSendContentAsync(Documento documento, CancellationToken cancellationToken = default);
}
