using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Interfaces;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;

namespace IntegracaoItera.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Tags("IntraCred - Endpoints Diretos")]
public class ClientApiController(IClientService clientService ) : ControllerBase
{
    private readonly IClientService _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));

    [HttpPost("UploadDocument")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadDocument(
        [FromForm] ClientRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.Arquivos == null || request.Arquivos.Count == 0)
        {
            return BadRequest("O arquivo é obrigatório e não pode estar vazio.");
        }

        if (string.IsNullOrWhiteSpace(request.Cnpj))
        {
            return BadRequest("O CNPJ é obrigatório.");
        }

        var result = await _clientService.SetContentAsync( request, cancellationToken );
        

        return Ok(result);
    }

    [HttpPost("Result")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Result(
        [FromForm] string Cnpj,
        CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(Cnpj))
        {
            return BadRequest("O CNPJ é obrigatório.");
        }

        var result = await _clientService.SendResultAsync(Cnpj, cancellationToken);


        return Ok(result);
    }

}
