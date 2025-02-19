using h.Primitives.Games;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace h.Server.Infrastructure.Database.Configurations.Converters;

public class BoardMatrixConverter : ValueConverter<GameSymbol[][], string>
{
    public BoardMatrixConverter()
        : base(
            convertToProviderExpression:
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
            
            convertFromProviderExpression:
                v => JsonSerializer.Deserialize<GameSymbol[][]>(v, JsonSerializerOptions.Default)!
        )
    { }
}