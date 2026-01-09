namespace IntegracaoItera.Data.Enums;

public enum ClientArquivoTipo
{
    /// <summary>
    /// Balanço que contém DRE
    /// </summary>
    BalancoComDre = 1,

    /// <summary>
    /// Balancete que contém DRE
    /// </summary>
    BalanceteComDre = 2,

    /// <summary>
    /// Documento contendo apenas a DRE.
    /// Não pode ser enviado isoladamente.
    /// </summary>
    Dre = 9
}
