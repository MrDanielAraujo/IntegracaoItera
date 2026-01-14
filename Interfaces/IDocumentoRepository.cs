using IntegracaoItera.Models;

namespace IntegracaoItera.Interfaces;

public interface IDocumentoRepository
{
    Task AdicionarAsync(Documento documento, CancellationToken cancellationToken);

    Task AtualizarAsync(Documento documento, CancellationToken cancellationToken);

    Task<Documento?> ObterPorIdAsync(Guid documentoId, CancellationToken cancellationToken);

    Task<Documento?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Documento>> ObterPendentesProcessamentoAsync(
        CancellationToken cancellationToken);

    Task<List<Documento>?> ObterPorStatusjAsync(CancellationToken cancellationToken);
}
