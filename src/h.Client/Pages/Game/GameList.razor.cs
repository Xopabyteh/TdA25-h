using h.Client.Services;
using h.Client.Services.Game;
using h.Contracts.Games;
using h.Primitives.Games;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.Data;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Game;

public partial class GameList
{
    [Inject] protected IWasmHttpClient _HttpClient { get; set; } = null!;
    [Inject] protected IWasmGameService _GameService { get; set; } = null!;

    private List<GameResponse>? games;
    private FilterModel filter = new(); // Filters to be used
    private FilterModel appliedFilter = new(); // Copied from filters when applied
    private Virtualize<GameResponse> virtualizeRef;

    protected override void OnInitialized()
    {
        games = new()
        {
            new(Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                "Armagedon",
                Primitives.Games.GameDifficulty.Easy,
                Primitives.Games.GameState.Opening,
                [
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "X", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "O", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "X", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "X", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "O", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                ]),
            new(Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                "Armagedon",
                Primitives.Games.GameDifficulty.Hard,
                Primitives.Games.GameState.Opening,
                [
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                ]),
            new(Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                "Armagedon",
                Primitives.Games.GameDifficulty.Hard,
                Primitives.Games.GameState.Opening,
                [
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                ]),
            new(Guid.NewGuid(),
                DateTime.Now,
                DateTime.Now,
                "Armagedon",
                Primitives.Games.GameDifficulty.Hard,
                Primitives.Games.GameState.Opening,
                [
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                ]),
        };
    }

    private async Task HandleGameDeleteClick(GameResponse game)
    {
        await _GameService.DeleteGameAsync(game.Uuid);
        
        games = await _GameService.LoadAllGamesAsync();
        await virtualizeRef.RefreshDataAsync();
    }

    private async ValueTask<ItemsProviderResult<GameResponse>> GetRows(ItemsProviderRequest request)
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return new ItemsProviderResult<GameResponse>(Array.Empty<GameResponse>(), 0); // Prerendering

        if (games is null)
        {
            games = await _GameService.LoadAllGamesAsync();
        }

        var filteredGames = appliedFilter.ApplyTo(games).ToArray();

        return new ItemsProviderResult<GameResponse>(
            filteredGames.Skip(request.StartIndex).Take(request.Count),
            filteredGames.Length
        );
    }

    private async Task HandleFilterClick()
    {
        appliedFilter = filter with {};
        await virtualizeRef.RefreshDataAsync();
    }

    private async Task HandleFilterReset()
    {
        filter = new();
        appliedFilter = new();
        await virtualizeRef.RefreshDataAsync();
    }

    public record FilterModel
    {
        public bool IsBeginner { get; set; } = false;
        public bool IsEasy { get; set; } = false;
        public bool IsMedium { get; set; } = false;
        public bool IsHard { get; set; } = false;
        public bool IsExtreme { get; set; } = false;
        public TimeSpan? UpdatedWithin { get; set; } = null;
        public string? GameName { get; set; } = null;

        public IEnumerable<GameResponse> ApplyTo(IEnumerable<GameResponse> arr)
        {
            return arr.Where(g => 
                (string.IsNullOrEmpty(GameName) || g.Name.Contains(GameName, StringComparison.OrdinalIgnoreCase)) &&
                (UpdatedWithin is null || g.UpdatedAt > DateTime.UtcNow - UpdatedWithin) &&
                MatchesDifficulty(g)
            );
        }

        private bool MatchesDifficulty(GameResponse game)
        {
            // If no difficulty filters are applied, return true
            if (!IsBeginner && !IsEasy && !IsMedium && !IsHard && !IsExtreme)
            {
                return true;
            }

            return (IsBeginner && game.Difficulty == GameDifficulty.Beginner) ||
                   (IsEasy && game.Difficulty == GameDifficulty.Easy) ||
                   (IsMedium && game.Difficulty == GameDifficulty.Medium) ||
                   (IsHard && game.Difficulty == GameDifficulty.Hard) ||
                   (IsExtreme && game.Difficulty == GameDifficulty.Extreme);
        }

    }
}
