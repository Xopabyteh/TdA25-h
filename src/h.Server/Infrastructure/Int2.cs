namespace h.Server.Infrastructure;

public readonly record struct Int2(int X, int Y)
{
    public static Int2 operator +(Int2 a, Int2 b) => new Int2(a.X + b.X, a.Y + b.Y);
    public static Int2 operator -(Int2 a, Int2 b) => new Int2(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Contains all orthogonal and diagonal directions (total 8...)
    /// </summary>
    public static Int2[] OrthoAndDiagonalDirections = new[]
    {
        new Int2(1, 0),
        new Int2(1, -1),
        new Int2(0, -1),
        new Int2(-1, -1),
        new Int2(-1, 0),
        new Int2(-1, 1),
        new Int2(0, 1),
        new Int2(1, 1)
    };
}