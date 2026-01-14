using IntegracaoItera.Common;
using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Data.Enums;
using IntegracaoItera.Interfaces;
using IntegracaoItera.Models;
using System.Text.Json;

namespace IntegracaoItera.Services;

public class ClientServerService(IApiClient apiClient, IApiServer apiServer, IDocumentoRepository documentoService, IDocumentoValidadorService documentoValidadorService) : IClientServerService
{

    private readonly IApiClient _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    private readonly IApiServer _apiServer = apiServer ?? throw new ArgumentNullException(nameof(apiServer));
    private readonly IDocumentoRepository _documentoService = documentoService ?? throw new ArgumentNullException(nameof(documentoService));
    private readonly IDocumentoValidadorService _documentoValidadorService = documentoValidadorService ?? throw new ArgumentNullException(nameof(documentoValidadorService));
    
    /// <summary>
    /// Esse é o metodo que finaliza o processo.
    /// Enviando o que foi retornado do servidor para o cliente.
    /// </summary>
    /// <param name="cnpj"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<MensagemRetornoDto> ClientSendResultAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(cnpj)) return new MensagemRetornoDto(400, "O Cnpj é obrigatório!");

        if (!CnpjValidator.IsValid(cnpj)) return new MensagemRetornoDto(400, "O Cnpj informado é invalido");

        // vai buscar o documento no banco de dados.
        var documento = await _documentoService.ObterPorCnpjAsync(cnpj, cancellationToken);

        // verifica se o documento foi encontrado.
        if (documento == null) return new MensagemRetornoDto(400, "O Documento não foi encontrado!");
        // verifica se o documento já foi processado
        if (documento!.ServerStatus != (int)ServerStatus.Concluido) return new MensagemRetornoDto(400, "O documento ainda não se encontra processado!");
        // verifica se o documento já foi enviado ao cliente.
        if (documento!.ClientStatus == (int)ClientStatus.Devolvido) return new MensagemRetornoDto(400, "Esse documento já foi enviado ao cliente!");

        // Cria uma nova instancia do objeto que o cliente esta aguardadno.
        var clientResponseDto = new ClientResponseDto()
        {
            Cnpj = documento.ClientCnpj,
            Result = documento.ServerResult!,
        };

        try
        {
            // envia o documento para o cliente, pelo endpoit da api do cliente.
            var result = await _apiClient.SetResultAsync(clientResponseDto, cancellationToken);

            // altera o status do documento com devolvido.
            documento.ClientStatus = (int)ClientStatus.Devolvido;

            // altera o banco de dados com o novo status.
            await _documentoService.AtualizarAsync(documento, cancellationToken);
        }
        catch (Exception ex)
        {
            // informa que deu erro no processo.
            return new MensagemRetornoDto(400, ex.Message);
        }

        // retorna mensagem de processo concluido com sucesso.
        return new MensagemRetornoDto(200, "success");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<MensagemRetornoDto> ClientSetContentAsync(ClientRequestDto request, CancellationToken cancellationToken = default)
    {
        if (!_documentoValidadorService.ValidarCnpj(request.Cnpj)) return new MensagemRetornoDto(400, "O Cnpj informado é invalido");

        ClientArquivoDto arquivoFinal;

        try
        {
            arquivoFinal = _documentoValidadorService.ResolverListaDeArquivosParaEnvio(request.Arquivos);
        }
        catch (Exception ex)
        {
            return new MensagemRetornoDto(400, ex.Message);
        }

        // Cria o novo documento.
        var documento = new Documento()
        {
            Id = Guid.NewGuid(),
            ClientCnpj = request.Cnpj,
            ClientArquivoAno = arquivoFinal.Ano,
            ClientArquivoTipo = arquivoFinal.Tipo,
            ClientArquivoNome = arquivoFinal.Nome,
            ClientArquivoContent = arquivoFinal.Content,
            ClientArquivoDataCadastro = arquivoFinal.DataCadastro,
            ClientStatus = (int)ClientStatus.Recebido,
            ServerStatus = (int)ServerStatus.Recebido
        };

        try
        {
            // Inclui no banco de dados o novo documento.
            await _documentoService.AdicionarAsync(documento, cancellationToken);

            // Chama o metodo que vai enviar o arquivo para o servidor analizar.
            await ServerSendContentAsync(documento, cancellationToken);

        }
        catch (Exception ex)
        {
            // retorna com a menagem de erro recebida.
            return new MensagemRetornoDto(400, ex.Message);
        }

        // retorna com uma mensagem de sucesso. 
        return new MensagemRetornoDto(200, "success");

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="documento"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<MensagemRetornoDto> ServerCheckStatusAsync(Documento documento, CancellationToken cancellationToken = default)
    {
        try
        {
            // vai na api do servidor para obter o status de processamento. 
            var retorno = await _apiServer.GetStatusAsync(documento.ServerId, cancellationToken);

            // verifica se o resultado voltou diferente de concluido.
            if (!retorno!.status.Equals("Concluido", StringComparison.OrdinalIgnoreCase))
            {
                // altera o status para processando.
                documento.ServerStatus = (int)ServerStatus.Processando;
                // persiste no banco de dados.
                await _documentoService.AtualizarAsync(documento, cancellationToken);
                // Sai da checagem informando que ainda não foi concluido.
                return new MensagemRetornoDto(400, "O Servidor ainda não concluiu o processamento!");
            }
            
            //grava satatus no banco de dados 
            documento.ServerStatus = (int)ServerStatus.ProcessadoComSucesso;

            //atualiza no vanco de dados o documento com o novo status.
            await _documentoService.AtualizarAsync(documento, cancellationToken);

            // chama o metodo que vai obter o conteudo processado. 
            return await ServerGetResultAsync(documento, cancellationToken);
        }
        catch (Exception ex)
        {
            return new MensagemRetornoDto(400, ex.Message);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="documento"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<MensagemRetornoDto> ServerGetResultAsync(Documento documento, CancellationToken cancellationToken = default)
    {
        try
        {
            // vai no servidor buscar o conteudo processado
            var exportJson = await _apiServer.GetExportJsonAsync(long.Parse(documento.ClientCnpj), cancellationToken);

            // converteu o conteudo em um tipo json string
            documento.ServerResult = JsonSerializer.Serialize(exportJson);

            documento.ServerStatus = (int)ServerStatus.Concluido;
            // atualizou o conteudo do docuemnto no banco. 
            await documentoService.AtualizarAsync(documento, cancellationToken);

            // chama o metodo que vai enviar o doumento para o client.
            return await ClientSendResultAsync(documento.ClientCnpj, cancellationToken);

        }
        catch (Exception ex)
        {
            return new MensagemRetornoDto(400, ex.Message);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="documento"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<MensagemRetornoDto> ServerSendContentAsync(Documento? documento, CancellationToken cancellationToken = default)
    {
        // verifica se o objeto documento foi enviado corretamente.
        if (documento is null) return new MensagemRetornoDto(400, "Documento não encontrado!");

        // cria uma instancia para o arquivo para ser enviado na api do servidor.
        IFormFile formFile;

        // carrega o arquivo que estava em byte[] para o formato de Envio por formulario.
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
            // Faz o envio do arquivo via Http post para o Endpoint do Servidor.
            var httpResponse = await _apiServer.UploadDocumentAsync( formFile, documento.ClientArquivoNome, "", documento.ClientCnpj, cancellationToken );

            // Obtem o resultado e converte para um tipo valido de chave Guid.
            Guid serverId = Guid.Parse(httpResponse);

            // Atualiza o objeto Documento para o Status de Enviado para o Servidor.
            documento.ServerStatus = (int)ServerStatus.EnviadoParaServer;
            
            // Atualiza o documento com o valor de Guid recebido apoz o envio ao servidor.
            documento.ServerId = serverId;
            
            // Efetua a alteração no banco de dados. 
            await _documentoService.AtualizarAsync(documento, cancellationToken);

            // Inicia o processo de verificação de status.
            /// return await ServerCheckStatusAsync(documento, cancellationToken);
        }
        catch (Exception ex)
        {
            // Caso algo de errado ao enviar ao servidor ou até mesmo no processo de chack
            // Será atualizado o documento com o status de erro.
            documento.ServerStatus = (int)ServerStatus.Erro;
            
            // atualiza o documento com a mensagem de erro recebida.
            documento.ServerMessage = ex.Message;
            
            // atauliza no banco de dados o documento.
            await _documentoService.AtualizarAsync(documento, cancellationToken);

            // retorna com a informação de erro.
            return new MensagemRetornoDto(400, ex.Message);
        }

        return new MensagemRetornoDto(200, "sucess");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="documento"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<MensagemRetornoDto> ServerResendContentAsync(Guid documentoId, CancellationToken cancellationToken = default)
    {
        // Obtem o documento atravez do ID
        var documento = await _documentoService.ObterPorIdAsync(documentoId, cancellationToken);
        
        // verifica se o objeto documento foi enviado corretamente.
        if (documento is null) return new MensagemRetornoDto(400, "Documento não encontrado!");

        // Casos que o Documento não pode ser reenviados.
        if (documento.ServerStatus == (int)ServerStatus.Concluido ||
            documento.ServerStatus == (int)ServerStatus.Processando  ||
            documento.ServerStatus == (int)ServerStatus.EnviadoParaServer ||
            documento.ClientStatus == (int)ClientStatus.Devolvido) return new MensagemRetornoDto(400, "Esse documento não pode ser reenviado!");

        // Limpa os campos para um novo reenvio.
        documento.ServerStatus = (int)ServerStatus.Recebido;
        documento.ServerMessage = string.Empty;
        documento.ServerId = Guid.Empty;
        documento.ServerResult = string.Empty;

        // Atauliza o documento no banco.
        await _documentoService.AtualizarAsync(documento, cancellationToken);

        // Clama o metodo que vai reenviar o documento ao servidor.
        return await ServerSendContentAsync(documento, cancellationToken);
    }
}
