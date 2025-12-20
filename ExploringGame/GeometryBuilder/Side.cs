namespace ExploringGame.GeometryBuilder;

/// <summary>
/// North = +Z
/// South = -Z
/// West = -X
/// East = X
/// </summary>
public enum Side
{
    North,
    NorthWest,
    West,
    SouthWest,
    South,
    SouthEast,
    East,
    NorthEast
}

public static class SideExtensions
{
    public static Side[] Decompose(this Side side) =>
        side switch
        {
            Side.North => new[] { Side.North },
            Side.NorthWest => new[] { Side.North, Side.West },
            Side.West => new[] { Side.West },
            Side.SouthWest => new[] { Side.South, Side.West },
            Side.South => new[] { Side.South },
            Side.SouthEast => new[] { Side.South, Side.East },
            Side.East => new[] { Side.East },
            Side.NorthEast => new[] { Side.North, Side.East },
            _ => new[] { side }
        };

    public static Side Opposite(this Side side) =>
        side switch
        {
            Side.North => Side.South,
            Side.NorthWest => Side.SouthEast,
            Side.West => Side.East,
            Side.SouthWest => Side.NorthEast,
            Side.South => Side.North,
            Side.SouthEast => Side.NorthWest,
            Side.East => Side.West,
            Side.NorthEast => Side.SouthWest,
            _ => side
        };    
}