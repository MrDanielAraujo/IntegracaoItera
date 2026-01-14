using IntegracaoItera.Data;
using IntegracaoItera.Data.Enums;
using IntegracaoItera.Interfaces;
using IntegracaoItera.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IntegracaoItera.Services;

public class DocumentoService(IntegraDbContext context) : IDocumentoRepository
{

    private readonly IntegraDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task AdicionarAsync(Documento documento, CancellationToken cancellationToken)
    {
        await _context.Set<Documento>()
                .AddAsync(documento, cancellationToken);
        _context.SaveChanges();
    }

    public Task AtualizarAsync(Documento documento, CancellationToken cancellationToken)
    {
        _context.Set<Documento>().Update(documento);
        _context.SaveChanges();
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<Documento>> ObterPendentesProcessamentoAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Documento?> ObterPorIdAsync(Guid documentoId, CancellationToken cancellationToken)
    {
        return await _context.Set<Documento>()
                .FirstOrDefaultAsync(
                    d => d.Id == documentoId,
                    cancellationToken);
    }

    public async Task<Documento?> ObterPorCnpjAsync(string cnpj, CancellationToken cancellationToken)
    {
        return await _context.Set<Documento>()
                .FirstOrDefaultAsync(
                    d => d.ClientCnpj == cnpj,
                    cancellationToken);
    }

    public async Task<List<Documento>?> ObterPorStatusjAsync(CancellationToken cancellationToken)
    {
        return await _context.Set<Documento>()
                .Where(
                    d => d.ServerStatus == (int)ServerStatus.EnviadoParaServer || 
                    d.ServerStatus == (int)ServerStatus.Processando
                    ).ToListAsync(cancellationToken);
    }
}
