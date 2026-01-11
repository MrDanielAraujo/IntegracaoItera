namespace IntegracaoItera.Data.Enums;

public enum ServerStatus
{
    /// <summary>
    /// Documento recebido do Cliente e persistido,
    /// mas ainda não enviado para a Server.
    /// </summary>
    Recebido = 1,

    /// <summary>
    /// Documento enviado para a Server e aguardando
    /// início do processamento.
    /// </summary>
    EnviadoParaServer = 2,

    /// <summary>
    /// Documento em processamento no Server.
    /// Status monitorado via polling.
    /// </summary>
    Processando = 3,

    /// <summary>
    /// Documento processado com sucesso
    /// e resultado disponível.
    /// </summary>
    ProcessadoComSucesso = 4,

    /// <summary>
    /// Erro ocorrido durante o processamento
    /// ou comunicação com o Server.
    /// </summary>
    Erro = 5
}
