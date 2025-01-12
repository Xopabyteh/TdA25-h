using Microsoft.AspNetCore.Components;

namespace h.Client.Pages.Game;
public partial class GameIndex
{
    [Parameter]
    public Guid? GameId { get; set; }
}