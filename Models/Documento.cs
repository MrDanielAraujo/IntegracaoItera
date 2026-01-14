using System.ComponentModel.DataAnnotations.Schema;

namespace IntegracaoItera.Models;

[Table("IteraControle")]
public class Documento
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
    public string ClientArquivoDataCadastro { get; set; } = null!;
    public byte[] ClientArquivoContent { get; set; } = null!;
}
