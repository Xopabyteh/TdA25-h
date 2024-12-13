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

public static class GameSymbolParser
{
    public static GameSymbol Parse(char symbol)
    {
        return symbol switch
        {
            'X' => GameSymbol.X,
            'O' => GameSymbol.O,
            _ => GameSymbol.None
        };
    }
}