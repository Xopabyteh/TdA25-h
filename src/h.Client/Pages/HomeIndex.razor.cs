﻿using h.Contracts.Components.Services;
using h.Contracts.Games;
using Microsoft.AspNetCore.Components;

namespace h.Client.Pages;

public partial class HomeIndex
{
    [Inject] protected IWasmOnlyHttpClient _HttpClient { get; set; } = null!;
    private List<GameResponse> games;

    //protected override async Task OnInitializedAsync()
    //{
    //    var game = new GameResponse();
    //    //var res = await _HttpClient.GetAsync("/api/games");
    //}
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
                "Armagedont",
                Primitives.Games.GameDifficulty.Medium,
                Primitives.Games.GameState.Opening,
                [
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["O", "X", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "X", "X", "", "", "", "", "", ""],
                    ["", "", "", "", "", "", "", "O", "O", "", "", "", "", "", ""],
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

}
