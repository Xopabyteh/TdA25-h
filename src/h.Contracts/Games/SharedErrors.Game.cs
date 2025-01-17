using ErrorOr;

namespace h.Contracts;
public static partial class SharedErrors
{
    public static class Game
    {
        public static Error UnbalancedSymbolAmountError()
            => Error.Validation(nameof(UnbalancedSymbolAmountError), "The amount of X and O symbols is not balanced. The amounts must be same, or one off maximum");

        public static Error IncorrectStartingSymbolError()
            => Error.Validation(nameof(IncorrectStartingSymbolError), "The game must start with X symbol");
    }
}
