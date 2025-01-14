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
    [Inject] protected IWasmGameService _GameService { get; set; } = null!;
    [Inject] protected NavigationManager _NavigationManager { get; set; } = null!;

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

    protected override async Task OnInitializedAsync()
    {       
        if(RuntimeInformation.ProcessArchitecture != Architecture.Wasm)
            return; // Prerendering should not load game

        try
        {
            // Try load game if we have a GameId
            if(GameId is not null)
            {
                loadedGame = await _GameService.LoadGameAsync(GameId.Value);

                // If null, we have an invalid GameId
                if (loadedGame is null)
                {
                    _NavigationManager.NavigateTo(PageRoutes.Game.GameEditorWithParam(gameId: null));
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
    }

    public async Task HandleSelectO()
    {
        if (jsModule is null)
            return;

        await jsModule.InvokeVoidAsync("selectOPencil", disposeCts.Token);
    }

    public async Task HandleSelectEraser()
    {
        if (jsModule is null)
            return;

        await jsModule.InvokeVoidAsync("selectEraser", disposeCts.Token);
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

        // Todo: validate inputs
        
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

            var updateResult = await _GameService.UpdateGameAsync(request);
            // Todo: handle error
        } else
        {
            // Make new game
            var request = new CreateNewGameRequest(
                RequestModel.Name,
                GameDifficulty.FromValue(RequestModel.Difficulty),
                board);

            // Todo: handle error
            var result = await _GameService.CreateGameAsync(request);
            Console.WriteLine(result);
            GameId = result.Value.Uuid;
            loadedGame = result;
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
