using IntegracaoItera.Data;
using IntegracaoItera.Data.Enums;
using IntegracaoItera.Interfaces;
using IntegracaoItera.Models;
using Microsoft.EntityFrameworkCore;

namespace IntegracaoItera.Services;

/// <summary>
/// Classe que controla os metodos que acessão o banco, mais precisamente a classe Documento tabela "IteraControle".
/// </summary>
/// <param name="context"></param>
public class DocumentoService(IntegraDbContext context) : IDocumentoRepository
{

    private readonly IntegraDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Recebe um documento para percistir no banco de dados.
    /// </summary>
    /// <param name="documento"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task AdicionarAsync(Documento documento, CancellationToken cancellationToken)
    {
        // Inclui o registro no banco de dados
        await _context.Set<Documento>().AddAsync(documento, cancellationToken);
        
        // Salva o registro no banco de dados.
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Recebe um documento para ser alterado no banco de dados.
    /// </summary>
    /// <param name="documento"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task AtualizarAsync(Documento documento, CancellationToken cancellationToken)
    {
        // Envia o documento para o ser alterado no banco de dados.
        _context.Set<Documento>().Update(documento);
       
        // SAlva a alteração enviada no banco de dados.
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Busca no banco de dados um documento através do ID.
    /// </summary>
    /// <param name="documentoId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Documento?> ObterPorIdAsync(Guid documentoId, CancellationToken cancellationToken)
        => await _context.Set<Documento>().FirstOrDefaultAsync( d => d.Id == documentoId, cancellationToken);
    

    public async Task<Documento?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken)
        => await _context.Set<Documento>()
            .OrderByDescending( d => d.DataCriacao)
            .FirstOrDefaultAsync(d => d.ClientCnpj == cnpj,cancellationToken);
    
    public async Task<List<Documento>?> ObterPendentesProcessamentoAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<Documento>()
                .Where(
                    d => d.ServerStatus == (int)ServerStatus.EnviadoParaServer || 
                    d.ServerStatus == (int)ServerStatus.Processando
                    ).ToListAsync(cancellationToken);
    }
}
