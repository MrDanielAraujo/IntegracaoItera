namespace IntegracaoItera.Interfaces;

public interface IAnoDocumentoService
{
    /// <summary>
    /// Verifica se o ano informado é válido de acordo com a regra do ano base.
    /// </summary>
    bool AnoEhValido(int ano);

    /// <summary>
    /// Retorna a lista de anos válidos com base na regra atual.
    /// </summary>
    int[] ObterAnosValidos();
}
