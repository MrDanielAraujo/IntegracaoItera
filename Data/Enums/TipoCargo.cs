using System.ComponentModel.DataAnnotations;

namespace IntegracaoItera.Data.Enums;

public enum TipoCargo
{
    [Display(Name = "Administrador")]
    AdminSistema,
    [Display(Name = "Usuário")]
    Usuario,
    [Display(Name = "Gerente")]
    Gerente,
    [Display(Name = "Executivo")]
    Executivo,
    [Display(Name = "Acionista")]
    Acionista,
    [Display(Name = "Outros")]
    Outros,
}
