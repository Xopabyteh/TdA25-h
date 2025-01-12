using h.Contracts.Games;
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
    private IJSObjectReference? jsModule;
    private CancellationTokenSource disposeCts = new();

    private ElementReference gameFieldRef;

    /// <summary>
    /// Gets mapped to <see cref="UpdateGameRequest"/> if used for editing.
    /// </summary>
    public CreateNewGameRequest RequestModel { get; set; }

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

        var board = await jsModule.InvokeAsync<string[][]>("getGameField", disposeCts.Token);
    
    
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
    
        await jsModule.InvokeVoidAsync(
            "initializeEditor",
            disposeCts.Token,
            gameFieldRef);
    }

    public ValueTask DisposeAsync()
    {
        disposeCts.Cancel();
        disposeCts.Dispose();
        
        return jsModule?.DisposeAsync()
            ?? ValueTask.CompletedTask;
    }
}
