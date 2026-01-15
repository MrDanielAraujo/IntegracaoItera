using System.ComponentModel.DataAnnotations;

namespace IntegracaoItera.Data.Enums;

public enum UsuarioSetor
{
    [Display(Name = "T.I.")]
    TI,
    [Display(Name = "Mesa de Operações")]
    Mop,
    [Display(Name = "Comercial")]
    Comercial,
    [Display(Name = "Comitê")]
    Comite,
    [Display(Name = "Externo")]
    Externo,
    [Display(Name = "Outros")]
    Outros,
}
