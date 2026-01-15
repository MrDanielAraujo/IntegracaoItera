using IntegracaoItera.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntegracaoItera.Models;

public class Usuario
{
    public Guid Id { get; set; }
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Senha { get; set; }
    public string? Salt { get; set; }
    public bool? Ativo { get; set; } = true;
    [Column(TypeName = "text")]
    public TipoCargo Cargo { get; set; } = TipoCargo.Usuario;
    [Column(TypeName = "text")]
    public UsuarioSetor Setor { get; set; } = UsuarioSetor.Outros;
    [Column(TypeName = "text[]")]
    public List<string?>? CnpjVinculados { get; set; } = [];
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow.AddHours(-3);
}
