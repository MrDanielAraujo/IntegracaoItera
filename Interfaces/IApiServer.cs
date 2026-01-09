using IntegracaoItera.Data.DTOs;

namespace IntegracaoItera.Interfaces;

public interface IApiServer
{
    /// <summary>
    /// Obtém o status de um documento pelo ID.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Status do documento</returns>
    Task<ServerStatusResponseDto> GetStatusAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exporta dados em JSON pelo CNPJ.
    /// </summary>
    /// <param name="cnpj">CNPJ da empresa</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de dados da controladora</returns>
    Task<List<ServerExportJsonDto>> GetExportJsonAsync(long cnpj, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o mapeamento De-Para pelo ID.
    /// </summary>
    /// <param name="documentId">ID do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do mapeamento De-Para</returns>
    Task<string> GetDeParaAsync(Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza o upload de um documento para a API Itera.
    /// </summary>
    /// <param name="file">Arquivo a ser enviado</param>
    /// <param name="source">Origem do documento</param>
    /// <param name="description">Descrição do documento</param>
    /// <param name="cnpj">CNPJ da empresa</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta do upload contendo informações do documento criado</returns>
    Task<string> UploadDocumentAsync(
        IFormFile file,
        string source,
        string description,
        string cnpj,
        CancellationToken cancellationToken = default);
}
