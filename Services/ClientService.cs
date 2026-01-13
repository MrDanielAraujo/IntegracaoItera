using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Data.Enums;
using IntegracaoItera.Interfaces;
using IntegracaoItera.Models;

namespace IntegracaoItera.Services;

public class ClientService(IApiClient apiClient, IClientServerService clientServerService, IDocumentoRepository documentoService) : IClientService
{

    private readonly IApiClient _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    private readonly IClientServerService _clientServerService = clientServerService ?? throw new ArgumentNullException(nameof(clientServerService));
    private readonly IDocumentoRepository _documentoService = documentoService ?? throw new ArgumentNullException(nameof(documentoService));

    public async Task<MensagemRetornoDto> SendResultAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        var documento = await _documentoService.ObterPorCnpjAsync(cnpj, cancellationToken);

        if (documento == null) return new MensagemRetornoDto(400, "O Documento não foi encontrado!");
        if (documento!.ServerStatus != (int)ServerStatus.ProcessadoComSucesso ) return new MensagemRetornoDto(400, "O documento ainda não se encontra processado!");

        var clientResponseDto = new ClientResponseDto()
        {
            Cnpj = documento.ClientCnpj,
            Result = documento.ServerResult!,
        };

        try
        {
            var result = await _apiClient.SetResultAsync(clientResponseDto, cancellationToken);

            documento.ClientStatus = (int)ClientStatus.Devolvido;

            await _documentoService.AtualizarAsync(documento, cancellationToken);
        }
        catch (Exception ex)
        {
            return new MensagemRetornoDto(400, ex.Message);
        }


        return new MensagemRetornoDto(200, "success");
    }

    public async Task<bool> SetContentAsync(ClientRequestDto request, CancellationToken cancellationToken = default)
    {
        // 1) Validar os Dados.
        // 1.1) Filtrar os dados.
        
        var documento = new Documento()
        {
            Id = Guid.NewGuid(),
            ClientStatus = (int)ClientStatus.Recebido
        };

        await _documentoService.AdicionarAsync(documento, cancellationToken);

        
         await _clientServerService.ServerSendContentAsync(documento.Id, cancellationToken);

        throw new NotImplementedException();
    }

}

