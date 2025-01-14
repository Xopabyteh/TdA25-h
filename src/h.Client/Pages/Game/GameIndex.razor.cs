using Microsoft.AspNetCore.Components;
using System.Runtime.CompilerServices;

namespace h.Client.Pages.Game;
public partial class GameIndex
{
    [Parameter]
    public Guid? GameId { get; set; }

    private bool xTurn = true;
    private string imgSrc = "";
    private string imgAlt = "";
}