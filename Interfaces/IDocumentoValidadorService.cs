using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Models;

namespace IntegracaoItera.Interfaces;

public interface IDocumentoValidadorService
{
    bool ValidarCnpj(string? cnpj);
    ClientArquivoDto ResolverListaDeArquivosParaEnvio(List<ClientArquivoDto> arquivos);
}
