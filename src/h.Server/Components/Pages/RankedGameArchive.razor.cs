using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace h.Client.Pages.User;

public partial class GameArchive
{
    [Parameter] [FromRoute(Name = "gameId")] public int? gameId { get; set; }
    

}
