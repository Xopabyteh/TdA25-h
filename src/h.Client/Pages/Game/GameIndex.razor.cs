using h.Contracts.Games;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace h.Client.Pages.Game;

/// <summary>
/// The page where user plays the game.
/// He can start a fresh game or load a saved one.
/// </summary>
public partial class GameIndex
{
    [Parameter] public Guid? GameId { get; set; }
    
    [Inject] protected IJSRuntime _js { get; set; } = null!;

    private IJSObjectReference? jsModule;
    private CancellationTokenSource disposeCts = new();

    private ElementReference gameFieldRef;
    private DotNetObjectReference<GameIndex>? dotNetRef;

    private bool xOnTurn = true;
    private int moveI;
    private int turnI;
    private string turnDisplaySrc = "";
    private string turnDisplayAlt = "";

    /// <summary>
    /// Change to loaded game is not null
    /// </summary>
    private bool IsVisible { get; set; } = false;

    protected override void OnInitialized()
    {
        dotNetRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnInitializedAsync()
    {
        IsVisible = GameId is not null;
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

    [JSInvokable]
    public void OnMoved(bool xOnTurn, int moveI, int turnI)
    {
        this.xOnTurn = xOnTurn;
        this.moveI = moveI;
        this.turnI = turnI;
        StateHasChanged();
    }

    [JSInvokable]
    public void SetWinner(bool xWins)
    {
        Console.WriteLine(xWins);
    }
}