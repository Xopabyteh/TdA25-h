using h.Client.Services;
using h.Client.Services.Game;
using h.Contracts.Games;
using h.Primitives.Games;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace h.Client.Pages.Game;

/// <summary>
/// The page where user plays the game.
/// He can start a fresh game or load a saved one.
/// </summary>
public partial class GameIndex : IAsyncDisposable
{
    [Parameter] public Guid? GameId { get; set; }
    
    [Inject] protected IJSRuntime _js { get; set; } = null!;
    [Inject] protected IWasmGameService _gameService { get; set; } = null!;
    [Inject] protected ToastService _toastService { get; set; } = null!;
    [Inject] protected NavigationManager _navigationManager { get; set; } = null!;

    private IJSObjectReference? jsModule;
    private CancellationTokenSource disposeCts = new();

    private ElementReference gameFieldRef;
    private DotNetObjectReference<GameIndex>? dotNetRef;

    private GameResponse? LoadedGame;

    private bool xOnTurn = true;
    private int moveI;
    private int turnI;
    private string turnDisplaySrc = "";
    private string turnDisplayAlt = "";

    private bool showSaveGameDialog;
    private SaveGameModel saveGameModel = new();

    private bool showWinnerDialog;
    private bool didXWin;

    protected override void OnInitialized()
    {
        dotNetRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnInitializedAsync()
    {
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return; // Prerendering...

        if (GameId is null)
            return;

        LoadedGame = await _gameService.LoadGameAsync(GameId.Value);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!firstRender)
            return;

        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return; // Prerendering should not load js

        jsModule = await _js.InvokeAsync<IJSObjectReference>(
            "import",
            disposeCts.Token,
            "./Pages/Game/GameIndex.razor.js");

        //await gameLoadedTcs.Task; // Wait until game is loaded so we can send it to the js module
        await jsModule.InvokeVoidAsync(
            "initializeGame",
            disposeCts.Token,
            gameFieldRef,
            dotNetRef,
            15,
            15
            //loadedGame?.Board
        );
    }

    /// <summary>
    /// Updates interface to show who is on turn
    /// </summary>
    [JSInvokable]
    public void OnMoved(bool xOnTurn, int moveI)
    {
        this.xOnTurn = xOnTurn;
        this.moveI = moveI;
        this.turnI = moveI / 2; // When both players moved, turnI is incremented

        StateHasChanged();
    }

    /// <summary>
    /// Called when game reached end and winner is determined
    /// </summary>
    [JSInvokable]
    public void SetWinner(bool xWins)
    {
        didXWin = xWins;
        showWinnerDialog = true;
    
        StateHasChanged();
    }

    private async Task HandleSaveGame()
    {
        if(jsModule is null)
            return;

        if(saveGameModel.Name is null)
            return; // Handled by html validation...

        var board = await jsModule.InvokeAsync<string[][]>("getGameField", disposeCts.Token);

        var request = new CreateNewGameRequest(
            saveGameModel.Name,
            GameDifficulty.FromValue(saveGameModel.Difficulty),
            board
        );

        var result = await _gameService.CreateGameAsync(request);
        result.Switch(
            game =>
            {
                // Move to the new game
                _navigationManager.NavigateTo(
                    PageRoutes.Game.GameIndexWithParam(game.Uuid),
                    forceLoad: false,
                    replace: true);

                // Todo: show success
            },
            async error =>
            {
                await _toastService.ErrorAsync($"Chyba při ukládání hry, zkuste prosím později\n{error.Message}");
            }
        );
    }



    public ValueTask DisposeAsync()
    {
        disposeCts.Cancel();
        disposeCts.Dispose();

        dotNetRef?.Dispose();
        
        return jsModule?.DisposeAsync() ?? ValueTask.CompletedTask;
    }

    class SaveGameModel
    {
        public string? Name { get; set; } = string.Empty;
        public int Difficulty { get; set; } = (int)GameDifficulty.Enum.Medium;
    }
}