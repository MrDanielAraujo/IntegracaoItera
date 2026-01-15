using IntegracaoItera.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace IntegracaoItera.Services;

[AllowAnonymous]
public class AnoDocumentoService : IAnoDocumentoService
{
    public bool AnoEhValido(int ano)
    {
        return ObterAnosValidos().Contains(ano);
    }

    public int[] ObterAnosValidos()
    {
        var anoBase = ObterAnoBase();

        return Enumerable
            .Range(0, 4)
            .Select(i => anoBase - (3 - i))
            .ToArray();
    }

    private static int ObterAnoBase()
    {
        var hoje = DateTime.Now;
        return hoje.Month >= 5
            ? hoje.Year
            : hoje.Year - 1;
    }
}
