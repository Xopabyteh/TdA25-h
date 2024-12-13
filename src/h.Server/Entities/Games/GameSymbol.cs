using System.ComponentModel.DataAnnotations;

namespace h.Server.Entities.Games;

public enum GameSymbol
{
    [Display(Name = "")]
    None,

    [Display(Name = "X")]
    X,

    [Display(Name = "O")]
    O
}
