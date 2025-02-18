using System.ComponentModel.DataAnnotations;

namespace h.Primitives.Games;

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
        return char.ToUpperInvariant(symbol) switch
        {
            'X' => GameSymbol.X,
            'O' => GameSymbol.O,
            _ => GameSymbol.None
        };
    }
}