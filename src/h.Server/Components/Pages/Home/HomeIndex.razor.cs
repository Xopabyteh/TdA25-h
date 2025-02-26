
using h.Server.Infrastructure.Matchmaking;
using h.Server.Infrastructure.MultiplayerGames;

namespace h.Server.Components.Pages.Home;

public partial class HomeIndex
{
    private int? roomInviteCode;

    private int playersOnline;
    private int gamesInProgress;

    private readonly IMatchmakingQueueService _queueService;
    private readonly IMultiplayerGameSessionService _gameSessionService;

    public HomeIndex(IMatchmakingQueueService queueService, IMultiplayerGameSessionService gameSessionService)
    {
        _queueService = queueService;
        _gameSessionService = gameSessionService;
    }

    protected override void OnInitialized()
    {
        playersOnline = _queueService.GetQueueSize();
        gamesInProgress = _gameSessionService.GetActiveGamesCount();

        base.OnInitialized();
    }
}
