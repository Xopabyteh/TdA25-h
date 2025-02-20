using h.Contracts;
using h.Server.Entities.MultiplayerGames;
using h.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace h.Server.Infrastructure.MultiplayerGames;

public class MultiplayerGameStatisticsService
{
    private readonly AppDbContext _db;

    public MultiplayerGameStatisticsService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// If there was gues in game - a guest game - this does nothing.
    /// Add game to players histories and update their statistics (wins, losses, draws).
    /// </summary>
    /// <exception cref="MultiplayerGameSession.GameNotEndedYetException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="SharedErrors.User.UserNotFoundException"></exception>
    public async Task UpdateAndSavePlayerStatisticsAsync(
        MultiplayerGameSession fromGame)
    {
        if(!fromGame.GameEnded)
            throw new MultiplayerGameSession.GameNotEndedYetException();

        if(fromGame.EndResult is null)
            throw new ArgumentNullException(nameof(fromGame.EndResult));

        // If there is a guest player ingame, ignore statistics
        if(fromGame.Players.Any(u => u.IsGuest))
            return;

        // -> Non guest - ranked game

        var player1Id = fromGame.Players.ElementAt(0);
        var player2Id = fromGame.Players.ElementAt(1);

        var player1 = await _db.UsersDbSet.AsTracking().FirstOrDefaultAsync(u => u.Uuid == player1Id.UserId!.Value)
            ?? throw new SharedErrors.User.UserNotFoundException();
        var player2 = await _db.UsersDbSet.AsTracking().FirstOrDefaultAsync(u => u.Uuid == player2Id.UserId!.Value)
            ?? throw new SharedErrors.User.UserNotFoundException();

        // Save statistics
        if (fromGame.EndResult.IsDraw)
        {
            player1.DrawAmount++;
            player1.Elo = player1.Elo.EloAfterDraw(player1, player2);
            
            player2.DrawAmount++;
            player2.Elo = player2.Elo.EloAfterDraw(player2, player1);
        }
        else if (fromGame.EndResult.WinnerId == player1Id)
        {
            player1.WinAmount++;
            player1.Elo = player1.Elo.EloAfterWin(player1, player2);

            player2.LossAmount++;
            player2.Elo = player2.Elo.EloAfterLoss(player2, player1);
        }
        else
        {
            player1.LossAmount++;
            player1.Elo = player1.Elo.EloAfterLoss(player1, player2);

            player2.WinAmount++;
            player2.Elo = player2.Elo.EloAfterWin(player2, player1);
        }

        // Save game
        var finishedRankedGame = new FinishedRankedGame()
        {
            LastBoardState = fromGame.Board,
            PlayedAt = fromGame.CreatedAtUtc.UtcDateTime,
            Player1Id = player1Id.UserId!.Value,
            Player2Id = player2Id.UserId!.Value,
            Player1Symbol = fromGame.PlayerSymbols[player1Id],
            Player2Symbol = fromGame.PlayerSymbols[player2Id],
            Player1RemainingTimer = fromGame.GetRemainingTime(player1Id),
            Player2RemainingTimer = fromGame.GetRemainingTime(player2Id),
            IsDraw = fromGame.EndResult.IsDraw,
            WinnerId = fromGame.EndResult.WinnerId!.Value.UserId
        };

        _db.FinishedRankedGames.Add(finishedRankedGame);
        // Todo: what the fuck do we do if this fails?
        await _db.SaveChangesAsync(); // Save, so we have id for finished game

        _db.UserToFinishedRankedGames.AddRange([
            new() {
                    UserId = player1Id.UserId!.Value,
                    FinishedRankedGameId = finishedRankedGame.Id
                },
                new() {
                    UserId = player2Id.UserId!.Value,
                    FinishedRankedGameId = finishedRankedGame.Id
                }
        ]);

        // Todo: what the fuck do we do if this fails?
        await _db.SaveChangesAsync(); // Save finished game to user mappings
    }
}
