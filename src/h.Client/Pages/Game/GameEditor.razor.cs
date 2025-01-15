using h.Client.Services;
using h.Client.Services.Game;
using h.Contracts.Games;
using h.Primitives.Games;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Game;

/// <summary>
/// Handles new game creatn and editing of existing games.
/// </summary>
public partial class GameEditor : IAsyncDisposable
{
    [Parameter] public Guid? GameId { get; set; }

    [Inject] protected IJSRuntime _js { get; set; } = null!;
    [Inject] protected IWasmGameService _gameService { get; set; } = null!;
    [Inject] protected NavigationManager _navigationManager { get; set; } = null!;
    [Inject] protected ToastService _toastService { get; set; } = null!;


    private IJSObjectReference? jsModule;
    private CancellationTokenSource disposeCts = new();

    private ElementReference gameFieldRef;

    /// <summary>
    /// Loads when <see cref="GameId" is set/> and is the edited game.
    /// </summary>
    private GameResponse? loadedGame;
    private TaskCompletionSource gameLoadedTcs = new();

    /// <summary>
    /// Gets mapped to <see cref="UpdateGameRequest"/> if used for editing.
    /// </summary>
    private EditorModel RequestModel { get; set; } = new();

    private string imgSrc = "";
    
    private int ActiveButtonId { get; set; } = 0;

    // Set active button state
    private void SetActiveButton(int buttonId)
    {
        ActiveButtonId = buttonId;
    }

    /// <summary>
    /// Sets active class for the button
    /// </summary>
    private string GetBtnActiveClass(int buttonId)
        => ActiveButtonId == buttonId ? "active" : string.Empty;

    protected override async Task OnInitializedAsync()
    {       
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return; // Prerendering should not load game

        try
        {
            // Try load game if we have a GameId
            if(GameId is not null)
            {
                loadedGame = await _gameService.LoadGameAsync(GameId.Value);

                // If null, we have an invalid GameId
                if (loadedGame is null)
                {
                    _navigationManager.NavigateTo(PageRoutes.Game.GameEditorWithParam(gameId: null));
                    return;
                }

                RequestModel = new EditorModel
                {
                    Name = loadedGame.Value.Name,
                    Difficulty = loadedGame.Value.Difficulty
                };
            }
        } 
        finally
        {
            gameLoadedTcs.SetResult();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!firstRender)
            return;

        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return; // Prerendering should not load js

        jsModule = await _js.InvokeAsync<IJSObjectReference>(
            "import",
            disposeCts.Token, "./Pages/Game/GameEditor.razor.js");

        await gameLoadedTcs.Task; // Wait until game is loaded so we can send it to the js module
        await jsModule.InvokeVoidAsync(
            "initializeEditor",
            disposeCts.Token,
            gameFieldRef,
            15,
            15,
            loadedGame?.Board
        );
    }

    public async Task HandleSelectX()
    {
        if(jsModule is null)
            return;

        await jsModule.InvokeVoidAsync("selectXPencil", disposeCts.Token);
        imgSrc = "url('/IMG/X/X_cervene.svg')";
    
        SetActiveButton(1);
    }

    public async Task HandleSelectO()
    {
        if (jsModule is null)
            return;

        await jsModule.InvokeVoidAsync("selectOPencil", disposeCts.Token);
        imgSrc = "url('/IMG/O/O_modre.svg')";
    
        SetActiveButton(2);
    }

    public async Task HandleSelectEraser()
    {
        if (jsModule is null)
            return;

        await jsModule.InvokeVoidAsync("selectEraser", disposeCts.Token);
        imgSrc = "none";
    
        SetActiveButton(3);
    }

    public async Task HandleClearCanvas()
    {
        if (jsModule is null)
            return;

        await jsModule.InvokeVoidAsync("clearCanvas", disposeCts.Token);
    }

    public async Task HandleHistoryBack()
    {
        if (jsModule is null)
            return;

        await jsModule.InvokeVoidAsync("historyBack", disposeCts.Token);
    }

    public async Task HandleHistoryForth()
    {
        if (jsModule is null)
            return;

        await jsModule.InvokeVoidAsync("historyForth", disposeCts.Token);
    }

    public async Task HandleSaveGame()
    {
        if(jsModule is null)
            return;
        
        var board = await jsModule.InvokeAsync<string[][]>("getGameField", disposeCts.Token);

        // Check if we have a loaded game (we have a GameId)
        // or we are making a new game (GameId is null)
        if(GameId is not null)
        {
            // Update the loaded game
            var request = new UpdateGameRequest(
                RequestModel.Name,
                GameDifficulty.FromValue(RequestModel.Difficulty),
                board,
                GameId!.Value);

            var updateResult = await _gameService.UpdateGameAsync(request);
            updateResult.Switch(
                async game =>
                {
                    await _toastService.SuccessAsync("Uloženo");
                    return;
                },
                async error =>
                {
                    await _toastService.ErrorAsync(error.Message);
                }
            );
        } else
        {
            // Make new game
            var request = new CreateNewGameRequest(
                RequestModel.Name,
                GameDifficulty.FromValue(RequestModel.Difficulty),
                board);

            var result = await _gameService.CreateGameAsync(request);
            result.Switch(
                async game =>
                {
                    GameId = game.Uuid;
                    loadedGame = game;
                    _navigationManager.NavigateTo(
                        PageRoutes.Game.GameEditorWithParam(GameId),
                        forceLoad: false,
                        replace: false);
                    await _toastService.SuccessAsync("Uloženo");
                    return;
                },
                async error =>
                {
                    await _toastService.ErrorAsync(error.Message);
                }
            );
        }
    }


    public async ValueTask DisposeAsync()
    {
        disposeCts.Cancel();
        disposeCts.Dispose();

        if (jsModule is not null)
        {
            await jsModule.DisposeAsync();
        }
    }

    class EditorModel
    {
        public string? Name { get; set; } = string.Empty;
        public int Difficulty { get; set; } = (int)GameDifficulty.Enum.Medium;
    }
}
