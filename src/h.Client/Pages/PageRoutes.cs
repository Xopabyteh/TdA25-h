namespace h.Client.Pages;
public static class PageRoutes
{
    public const string HomeIndex = "/";
    public const string Leaderboard = "/leaderboard";

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
        public const string MultiplayerGame = "/multiplayer-game";
        public const string MatchmakingQueue = "/multiplayer-game/queue";
        
        public const string FriendMatchChooseType = "/multiplayer-game/friend-query";
        public const string FriendJoinCode = "/multiplayer-game/friend-invite";
        public static string FriendJoinCodeWithQuery(int? code) => $"{FriendJoinCode}?c={code}";

        public const string RoomWithCodeWaiting = "/multiplayer-game/friend-code";
    }
    
    public static class Auth
    {
        public const string LoginIndex = "/login";
        public static string LoginIndexWithQuery(string? @return) => $"{LoginIndex}?return={@return}";
        public const string RegisterIndex = "/register";
        public static string RegisterIndexWithQuery(string? @return) => $"{RegisterIndex}?return={@return}";
        public const string Logout = "/logout";
        public static string LogoutWithQuery(string? @return) => $"{Logout}?return={@return}";
    }
    
    public static class Admin
    {
        public const string AdminPanel = "/admin/panel";
        public const string Audit = "/admin/audit";
    }
    
    public static class User
    {
        public const string UserSettings = "/user/settings";
        public const string UserBoard = "/user/board/{userId:guid}";
        public static string UserBoardWithParam(Guid userId) => $"/user/board/{userId}";
        public const string UserGameHistory = "/user/game-history";
    }
}
