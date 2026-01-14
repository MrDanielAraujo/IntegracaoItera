using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegracaoItera.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Tags("Prossesamento Documentos")]
public class ProcessarDocumentosController(IClientServerService clientServerService) : ControllerBase
{
    private readonly IClientServerService _clientServerService = clientServerService ?? throw new ArgumentNullException(nameof(clientServerService));

    [HttpPost("Processar")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Processar([FromBody] ClientRequestDto request, CancellationToken cancellationToken)
    {
        var retorno = await _clientServerService.ClientSetContentAsync(request, cancellationToken);
        
        return (retorno.CodigoStatus != 200) ? BadRequest(retorno.DescricaoStatus) : Ok();
    }

    [HttpPost("ReProcessar")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReProcessar([FromBody] Guid documentId, CancellationToken cancellationToken)
    {
        var retorno = await _clientServerService.ServerResendContentAsync(documentId, cancellationToken);

        return (retorno.CodigoStatus != 200) ? BadRequest(retorno.DescricaoStatus) : Ok();
    }
}
    

    

