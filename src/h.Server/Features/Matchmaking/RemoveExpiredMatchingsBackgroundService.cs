using h.Contracts;
using h.Contracts.Matchmaking;
using h.Server.Infrastructure;
using h.Server.Infrastructure.Matchmaking;
using Microsoft.AspNetCore.SignalR;

namespace h.Server.Features.Matchmaking;

public class RemoveExpiredMatchingsBackgroundService : BackgroundService
{
    private const int RemoveExpiredEveryMs = 10_000;
    
    private readonly InMemoryMatchmakingService _matchmakingService;
    private readonly IMatchmakingQueueService _matchmakingQueueService;
    private readonly IHubUserIdMappingService<MatchmakingHub> _hubMapping;
    private readonly IHubContext<MatchmakingHub, IMatchmakingHubClient> _hubContext;
    private readonly ILogger<RemoveExpiredMatchingsBackgroundService> _logger;

    private PeriodicTimer timer = new(TimeSpan.FromMilliseconds(RemoveExpiredEveryMs));

    public RemoveExpiredMatchingsBackgroundService(
        InMemoryMatchmakingService matchmakingService,
        IMatchmakingQueueService matchmakingQueueService,
        IHubUserIdMappingService<MatchmakingHub> hubMapping,
        IHubContext<MatchmakingHub, IMatchmakingHubClient> hubContext,
        ILogger<RemoveExpiredMatchingsBackgroundService> logger)
    {
        _matchmakingService = matchmakingService;
        _matchmakingQueueService = matchmakingQueueService;
        _hubMapping = hubMapping;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RemoveExpiredMatchings();
        }
    }

    internal async Task RemoveExpiredMatchings()
    {
        var expiredMatches = _matchmakingService.RemoveExpiredMatchings();
        if(expiredMatches.Count == 0)
            return;

        _logger.LogInformation("Removed {Count} expired matchings", expiredMatches.Count);

        // Requeue the hanging acceptees and notify them about the expired match
        foreach (var expiredMatch in expiredMatches)
        {
            // Requeue the acceptees
            foreach (var hangingAcceptee in expiredMatch.HangingAcceptees)
            {
                var requeueResult = _matchmakingQueueService.AddUserToStartOfQueue(hangingAcceptee);
                if(requeueResult.IsError)
                    throw new SharedErrors.Matchmaking.UserAlreadyInQueueException();
            }

            // Notify clients about the expired match
            var connectionIds = expiredMatch.Matching.GetPlayersInMatch()
                .Select(_hubMapping.GetConnectionId)
                .Where(connectionId => connectionId is not null)! // This might be a cause of error one day? hopefully not :)
                .ToArray<string>();

            await _hubContext.Clients.Clients(connectionIds).MatchCancelled(expiredMatch.Matching.Id);
        }
    }
}
