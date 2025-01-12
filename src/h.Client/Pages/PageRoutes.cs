namespace h.Client.Pages;
public static class PageRoutes
{
    /// <summary>
    /// Temporarily rerouted to <see cref="GameList"/>
    /// </summary>
    //public const string HomeIndex = "/";

    public static class Game
    {
        /// <summary>
        /// Playing a game
        /// </summary>
        public const string GameIndex = "/game/{gameId:guid?}";
        public static string GameIndexWithParam(Guid gameId) => $"/game/{gameId}";
    
        public const string GameList = "/game/list";
        public const string GameEditor = "/game/editor/{gameId:guid?}";
        public static string GameEditorWithParam(Guid gameId) => $"/game/editor/{gameId}";
    }
}
