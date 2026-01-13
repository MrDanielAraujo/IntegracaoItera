using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Models;

namespace IntegracaoItera.Interfaces;

public interface IDocumentoValidadorService
{
    ClientArquivoDto ResolverListaDeArquivosParaEnvio(ClientRequestDto requestDto);
}
