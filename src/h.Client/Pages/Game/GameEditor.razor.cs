using Microsoft.AspNetCore.Components;

namespace h.Client.Pages.Game;

public partial class GameEditor
{
    [Parameter]
    public Guid? GameId { get; set; }
}
