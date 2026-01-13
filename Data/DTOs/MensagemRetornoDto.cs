namespace IntegracaoItera.Data.DTOs;

public class MensagemRetornoDto(int codigoStatus, string descricaoStatus)
{
    public int CodigoStatus { get; set; } = codigoStatus;

    public string DescricaoStatus { get; set; } = descricaoStatus;
}
