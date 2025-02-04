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
    private GameResponse[] filteredGames = Array.Empty<GameResponse>();
    private FilterModel filter = new(); // Filters to be used
    private FilterModel appliedFilter = new(); // Copied from filters when applied
    private bool removingFilter = false;

    protected override Task OnInitializedAsync()
    {
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return Task.CompletedTask; // Prerendering should not load game

        return LoadFillteredGamesAsync();
    }

    private async Task HandleGameDeleteClick(GameResponse game)
    {
        await _GameService.DeleteGameAsync(game.Uuid);
        
        games = await _GameService.LoadAllGamesAsync();
        await LoadFillteredGamesAsync();
    }
        
    private async Task LoadFillteredGamesAsync()
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;

        if (games is null)
        {
            games = await _GameService.LoadAllGamesAsync();
        }

        filteredGames = appliedFilter.ApplyTo(games).ToArray();
    }

    private async Task HandleFilterClick()
    {
        appliedFilter = filter with {};
        await LoadFillteredGamesAsync();
    }

    private async Task HandleFilterReset()
    {
        filter = new();
        appliedFilter = new();
        await LoadFillteredGamesAsync();
    }

    private async Task RemoveFilterAsync(Action<FilterModel> modifyDelegate)
    {
        if (removingFilter)
            return;

        removingFilter = true;

        modifyDelegate(filter);
        await HandleFilterClick();
            
        removingFilter = false;
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
