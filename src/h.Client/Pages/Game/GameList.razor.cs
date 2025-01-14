using h.Client.Services;
using h.Contracts.Components.Services;
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
    [Inject] protected IWasmOnlyHttpClient _HttpClient { get; set; } = null!;

    private List<GameResponse>? games;
    private FilterModel filter = new(); // Filters to be used
    private FilterModel appliedFilter = new(); // Copied from filters when applied
    private Virtualize<GameResponse> virtualizeRef;

    protected override async Task OnInitializedAsync()
    {
        // If prerendering, do not fetch data
        if (RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return;

    }

    private Task<List<GameResponse>> LoadAllGames()
    {
        return _HttpClient.Http!.GetFromJsonAsync<List<GameResponse>>("api/v1/games", AppJsonOptions.WithConverters)!;
    }

    private async ValueTask<ItemsProviderResult<GameResponse>> GetRows(ItemsProviderRequest request)
    {
        if (RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return new ItemsProviderResult<GameResponse>(Array.Empty<GameResponse>(), 0); // Prerendering

        if (games is null)
        {
            games = await LoadAllGames();
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
