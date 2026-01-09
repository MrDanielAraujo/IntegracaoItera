namespace IntegracaoItera.Models;

public class IntegraArquivo
{
    // Controle interno
    public Guid Id { get; set; }
    
    // Controle d server.
    public Guid ServerId { get; set; }
    public int ServerStatus { get; set; }
    public string? ServerResult { get; set; } // JSON
    public string? ServerMessage { get; set; }

    // Identificação do cliente
    public string ClientCnpj { get; set; } = null!;

    // Controle do arquivo
    public int ClientStatus { get; set; }
    public int ClientArquivoAno { get; set; }
    public int ClientArquivoTipo { get; set; }
    public string ClientArquivoNome { get; set; } = null!;
    public DateTime ClientArquivoDataCadastro { get; set; }
    public byte[] ClientArquivoContent { get; set; } = null!;
}
