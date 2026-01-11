namespace IntegracaoItera.Data.DTOs;

public class ClientRequestDto
{
    public string Cnpj { get; set; } = null!;
    public List<ClientArquivoDto> Arquivos { get; set; } = [];
}
