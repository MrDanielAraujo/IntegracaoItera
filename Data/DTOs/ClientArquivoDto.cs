namespace IntegracaoItera.Data.DTOs;

public class ClientArquivoDto
{
    public int Ano { get; set; }
    public int Tipo { get; set; }
    public string Nome { get; set; } = null!;
    public byte[] Content { get; set; } = null!;
    public DateTime DataCadastro { get; set; }
}
