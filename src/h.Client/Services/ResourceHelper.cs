using h.Primitives.Games;

namespace h.Client.Services;

public static class ResourceHelper
{
    public static string GetDifficultyBulbImgSrc(GameDifficulty difficulty)
        => difficulty.EnumValue switch
        {
            GameDifficulty.Enum.Beginner => "IMG/Difficulties/Beginner/zarivka_beginner_modre.svg",
            GameDifficulty.Enum.Easy => "IMG/Difficulties/Easy/zarivka_easy_modre.svg",
            GameDifficulty.Enum.Medium => "IMG/Difficulties/Medium/zarivka_medium_modre.svg",
            GameDifficulty.Enum.Hard => "IMG/Difficulties/Hard/zarivka_hard_modre.svg",
            GameDifficulty.Enum.Extreme => "IMG/Difficulties/Extreme/zarivka_extreme_modre.svg",
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
        };

    public static string GetSymbolSrc(GameSymbol symbol)
        => symbol switch
        {
            GameSymbol.X=> "/IMG/X/X_cervene.svg",
            GameSymbol.O => "/IMG/O/O_modre.svg",
            _ => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, null)
        };

    public static string GetTrophyIcon(bool win)
        => win ? "/IMG/icons/win.svg" : "/IMG/icons/lose.svg";
}
