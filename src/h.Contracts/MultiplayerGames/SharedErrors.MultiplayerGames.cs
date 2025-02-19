namespace h.Contracts;

using ErrorOr;

public static partial class SharedErrors
{
    public static class MultiplayerGames
    {
        public static Error GameNotFound()
            => Error.NotFound(nameof(GameNotFound), "The specified game was not found");

        public class GameNotFoundException : Exception
        {
            public GameNotFoundException() : base("The specified game was not found") { }
        }

        //public static Error GameAlreadyStarted()
        //    => Error.Conflict(nameof(GameAlreadyStarted), "The game has already started");

        //public static Error GameAlreadyEnded()
        //    => Error.Conflict(nameof(GameAlreadyEnded), "The game has already ended");

        //public static Error PlayerAlreadyInGame()
        //    => Error.Conflict(nameof(PlayerAlreadyInGame), "The player is already in the game");

        //public static Error PlayerNotInGame()
        //    => Error.NotFound(nameof(PlayerNotInGame), "The player is not in the game");
    }
}
