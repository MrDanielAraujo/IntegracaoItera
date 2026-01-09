namespace IntegracaoItera.Data.DTOs;

public class ServerArquivoDto
{
    public IFormFile File { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Cnpj { get; set; } = null!;
}
