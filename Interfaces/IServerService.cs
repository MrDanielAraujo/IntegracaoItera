using IntegracaoItera.Data.DTOs;

namespace IntegracaoItera.Interfaces;

public interface IServerService
{
    // esse metodo vai enviar para o server os dados e gravar o retorno e o status no banco de dados.
    Task<string> SendContentAsync(CancellationToken cancellationToken = default);

    // vai verificar se o conteudo ja foi processado. se conteudo processado chamar o getResulAssync.
    Task<bool> CheckStatusAsync(CancellationToken cancellationToken = default);

    //obem o conteudo do server, grava no banco e chama o clientserver para devolver o conteudo. 
    Task<bool> GetResultAsync(CancellationToken cancellationToken = default);
}
