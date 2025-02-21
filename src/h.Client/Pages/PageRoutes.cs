namespace h.Client.Pages;
public static class PageRoutes
{
    public const string HomeIndex = "/";

    /// <summary>
    /// Singleplayer game
    /// </summary>
    public static class Game
    {
        /// <summary>
        /// Playing a game
        /// </summary>
        public const string GameIndex = "/game/{gameId:guid?}";
        public static string GameIndexWithParam(Guid? gameId) => $"/game/{gameId}";
    
        public const string GameList = "/game/list";
        public const string GameEditor = "/game/editor/{gameId:guid?}";
        public static string GameEditorWithParam(Guid? gameId) => $"/game/editor/{gameId}";
    }
    
    /// <summary>
    /// Multiplayer game session
    /// </summary>
    public static class Multiplayer /*Multiplayer game*/
    {
        public const string MultiplayerIndex = "/multiplayer-game";
        public const string MultiplayerQueue = "/multiplayer-game/queue";
        
        public const string FriendQuery = "/multiplayer-game/friend-query";
        public const string FriendInvite = "/multiplayer-game/friend-invite";
        public const string FriendCode = "/multiplayer-game/friend-code";
    }
    
    public static class Login
    {
        public const string LoginIndex = "/login";
        public const string RegisterIndex = "/register";
    }
    
    public static class Admin
    {
        public const string AdminPanel = "/admin/panel";
        public const string Audit = "/admin/audit";
    }
    
    public static class Settings
    {
        public const string UserSettings = "/settings";
    }
}
