using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Data.Enums;
using IntegracaoItera.Interfaces;
using IntegracaoItera.Models;
using System.Text.Json;

namespace IntegracaoItera.Services;

public class ServerService(IApiServer apiServer, IClientServerService clientServerService, IDocumentoRepository documentoService) : IServerService
{

    private readonly IApiServer _apiServer = apiServer ?? throw new ArgumentNullException(nameof(apiServer));
    private readonly IClientServerService _clientServerService = clientServerService ?? throw new ArgumentNullException(nameof(clientServerService));
    private readonly IDocumentoRepository _documentoService = documentoService ?? throw new ArgumentNullException(nameof(documentoService));

    public async Task<MensagemRetornoDto> CheckStatusAsync(Documento documento, CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _apiServer.GetStatusAsync(documento.ServerId, cancellationToken);

            
            //grava satatus no banco de dados 

            return await GetResultAsync(documento, cancellationToken);
        }
        catch (Exception ex)
        {
            return new MensagemRetornoDto(400, ex.Message);
        }
        

        throw new NotImplementedException();
    }

    public async Task<MensagemRetornoDto> GetResultAsync(Documento documento,  CancellationToken cancellationToken = default)
    {
        try
        {
            var exportJson = await _apiServer.GetExportJsonAsync(long.Parse(documento.ClientCnpj), cancellationToken);

            documento.ServerResult = JsonSerializer.Serialize(exportJson);

            await documentoService.AtualizarAsync(documento, cancellationToken);

            return await _clientServerService.ClientSendResultAsync(documento.ClientCnpj, cancellationToken);

        }
        catch (Exception ex)
        {
            return new MensagemRetornoDto(400, ex.Message);
        }
    }



    public async Task<MensagemRetornoDto> SendContentAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var documento = await _documentoService.ObterPorIdAsync(documentId, cancellationToken);

        if (documento == null) return new MensagemRetornoDto(400, "Documento não encontrado!");

        IFormFile formFile;

        using (var stream = new MemoryStream(documento!.ClientArquivoContent))
        {
            formFile = new FormFile(baseStream: stream,
               baseStreamOffset: 0,
               length: stream.Length,
               name: "file", // The name of the form field, typically "file"
               fileName: documento.ClientArquivoNome
            );
        }

       
        try
        {
            var httpResponse = await _apiServer.UploadDocumentAsync(
                formFile,
                documento.ClientArquivoNome,
                "",
                documento.ClientCnpj,
                cancellationToken
            );

            Guid serverId = Guid.Parse(httpResponse);

            documento.ServerStatus = (int)ServerStatus.EnviadoParaServer;
            documento.ServerId = serverId;

            await _documentoService.AtualizarAsync(documento, cancellationToken);

            // verificar se o metodo já esta ativo.
            return await CheckStatusAsync(documento, cancellationToken);
        }
        catch (Exception ex)
        {
            documento.ServerStatus = (int)ServerStatus.Erro;
            documento.ServerMessage = ex.Message;

            await _documentoService.AtualizarAsync(documento, cancellationToken);

            return new MensagemRetornoDto(400, ex.Message);
        }
    }
}
