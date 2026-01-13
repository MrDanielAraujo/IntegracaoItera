using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IntegracaoItera.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Tags("Prossesamento Documentos")]
public class ProcessarDocumentosController(IClientServerService clientService) : ControllerBase
{
    private readonly IClientServerService _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));

    [HttpPost("Processar")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Processar([FromBody] ClientRequestDto request, CancellationToken cancellationToken)
        => Ok(await _clientService.ClientSetContentAsync(request, cancellationToken));
}
    

    

