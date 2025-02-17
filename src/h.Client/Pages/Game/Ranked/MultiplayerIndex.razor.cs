using System.Globalization;

namespace h.Client.Pages.Game.Ranked;

public partial class MultiplayerIndex
{
    private bool xOnTurn = true;
    private string turnDisplaySrc = "";
    private string turnDisplayAlt = "";
    private int turnI = 1;

    private int elo1 = 100;
    private int elo2 = 100;
}