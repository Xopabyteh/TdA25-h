using h.Contracts;
using h.Contracts.Matchmaking;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Database;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Features.Matchmaking;

public class MatchPlayersBackgroundService : BackgroundService
{
    private const int MatchUsersEveryMs = 1000;

    private readonly IMatchmakingQueueService _matchmakingQueue;
    private readonly InMemoryMatchmakingService _matchmakingService;
    private readonly IHubContext<MatchmakingHub, IMatchmakingHubClient> _matchmakingHub;
    private readonly IHubUserIdMappingService<MatchmakingHub> _userIdMappingService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MatchPlayersBackgroundService> _logger;

    private PeriodicTimer timer = new(TimeSpan.FromMilliseconds(MatchUsersEveryMs));

    public MatchPlayersBackgroundService(
        IMatchmakingQueueService matchmakingQueue,
        InMemoryMatchmakingService matchmakingService,
        IHubContext<MatchmakingHub, IMatchmakingHubClient> matchmakingHub,
        IHubUserIdMappingService<MatchmakingHub> userIdMappingService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MatchPlayersBackgroundService> logger)
    {
        _matchmakingQueue = matchmakingQueue;
        _matchmakingService = matchmakingService;
        _matchmakingHub = matchmakingHub;
        _userIdMappingService = userIdMappingService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await MatchUsers(dbContext);
        }
    }

    /// <summary>
    /// Match users,
    /// Create unaccepted matching,
    /// Notify clients to accept.
    /// </summary>
    /// <remarks>
    /// internal for testing purposes.
    /// </remarks>
    internal async Task MatchUsers(AppDbContext dbContext)
    {
        // Try find matching
        var potentialMatching = _matchmakingQueue.MatchUsers();
        if (potentialMatching is null)
            return;

        // Register new matching
        var matching = _matchmakingService.RegisterNewPlayerMatching(
            potentialMatching.Value.user1Id,
            potentialMatching.Value.user2Id
        );
        _logger.LogInformation("Matched players {Player1Id} and {Player2Id} in matching {MatchingId}",
            matching.Player1Id, matching.Player2Id, matching.Id);

        // Find users
        var user1 = await dbContext.UsersDbSet
            .FirstOrDefaultAsync(u => u.Uuid == matching.Player1Id);

        var user2 = await dbContext.UsersDbSet
            .FirstOrDefaultAsync(u => u.Uuid == matching.Player2Id);

        if(user1 is null || user2 is null)
            throw new SharedErrors.User.UserNotFoundException();

        // Notify clients
        var user1ConnectionId = _userIdMappingService.GetConnectionId(potentialMatching.Value.user1Id);
        var user2ConnectionId = _userIdMappingService.GetConnectionId(potentialMatching.Value.user2Id);

        // Ensure both players are in mapping, if not, remove matching
        // and requeue the unproblematic player (ignore the problematic one - leave him outside queue)
        if (user1ConnectionId is null || user2ConnectionId is null)
        {
            _logger.LogInformation("One of the players is not connected, removing matching {MatchingId}", matching.Id);
            _matchmakingService.RemovePlayerMatching(matching.Id);

            if (user1ConnectionId is not null)
            {
                _logger.LogInformation("Requeuing player {PlayerId} to the start", potentialMatching.Value.user1Id);
                var requeueResult = _matchmakingQueue.AddUserToStartOfQueue(potentialMatching.Value.user1Id);
                if (requeueResult.IsError)
                    throw new SharedErrors.Matchmaking.UserAlreadyInQueueException();
            }
            
            if (user2ConnectionId is not null)
            {
                _logger.LogInformation("Requeuing player {PlayerId} to the start", potentialMatching.Value.user2Id);
                var requeueResult = _matchmakingQueue.AddUserToStartOfQueue(potentialMatching.Value.user2Id);
                if (requeueResult.IsError)
                    throw new SharedErrors.Matchmaking.UserAlreadyInQueueException();
            }
            
            return;
        }

        // Notify clients
        string[] connectionIds = [user1ConnectionId, user2ConnectionId];
        
        await _matchmakingHub.Clients
            .Clients(connectionIds)
            .MatchFound(new(
                matching.Id,
                new(
                    user1.Uuid,
                    user1.Username,
                    user1.Elo.Rating
                ),
                new(
                    user2.Uuid,
                    user2.Username,
                    user2.Elo.Rating
                )
            ));
    }

    public override void Dispose()
    {
        timer.Dispose();
        base.Dispose();
    }
}
