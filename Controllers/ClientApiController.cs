using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Data.Enums;
using IntegracaoItera.Interfaces;
using IntegracaoItera.Models;
using IntegracaoItera.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace IntegracaoItera.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Tags("IntraCred - Endpoints Diretos")]
public class ClientApiController(IDocumentoValidadorService documentoValidadorService, IApiClient apiClient, IDocumentoRepository documentoService) : ControllerBase
{
    private readonly IDocumentoValidadorService _documentoValidadorService = documentoValidadorService ?? throw new ArgumentNullException(nameof(documentoValidadorService));
    private readonly IApiClient _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    private readonly IDocumentoRepository _documentoService = documentoService ?? throw new ArgumentNullException(nameof(documentoService));

    [HttpPost("UploadDocument")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadDocument(
        [FromBody] ClientRequestDto request,
        CancellationToken cancellationToken)
    {
        var arquivoFinal = _documentoValidadorService.ResolverListaDeArquivosParaEnvio(request);

        var documento = new Documento()
        {
            Id = Guid.NewGuid(),
            ClientCnpj = request.Cnpj,
            ClientArquivoAno = arquivoFinal.Ano,
            ClientArquivoTipo = arquivoFinal.Tipo,
            ClientArquivoNome = arquivoFinal.Nome,
            ClientArquivoContent = arquivoFinal.Content,
            ClientArquivoDataCadastro = arquivoFinal.DataCadastro,
            ClientStatus = (int)ClientStatus.Recebido
        };


        return Ok(documento);
    }

    [HttpPost("EnviaConteudoProcessado")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendContentProcessed([FromBody] string cnpj, CancellationToken cancellationToken)
    {
        // verifica se o cnpj foi enviado.
        if (string.IsNullOrEmpty(cnpj)) return BadRequest(new MensagemRetornoDto(400, "O Cnpj é obrigatório!"));

        // vai buscar o documento no banco de dados.
        var documento = await _documentoService.ObterPorCnpjAsync(cnpj, cancellationToken);

        // verifica se o documento foi encontrado.
        if (documento == null) return BadRequest(new MensagemRetornoDto(400, "O Documento não foi encontrado!"));

        // Cria uma nova instancia do objeto que o cliente esta aguardadno.
        var clientResponseDto = new ClientResponseDto()
        {
            Cnpj = documento.ClientCnpj,
            Result = documento.ServerResult!,
        };

        return Ok(await _apiClient.SetResultAsync(clientResponseDto, cancellationToken));
    }
        
}
