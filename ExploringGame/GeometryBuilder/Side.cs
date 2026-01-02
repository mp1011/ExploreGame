using System;

namespace ExploringGame.GeometryBuilder;

/// <summary>
/// North = -Z
/// South = +Z
/// West = -X
/// East = X
/// </summary>
[Flags]
public enum Side
{
    North = 1,
    West = 2,
    South = 4,
    East = 8,
    Bottom = 16,
    Top = 32,
    NorthWest = North | West,
    SouthWest = South | West,
    SouthEast = South | East,
    NorthEast = North | East,
    All = North | South | East | West | Top | Bottom
}

public static class SideExtensions
{
    public static Axis GetAxis(this Side side) =>
        side switch
        {
            Side.North => Axis.Z,
            Side.South => Axis.Z,
            Side.West => Axis.X,
            Side.East => Axis.X,
            Side.Bottom => Axis.Y,
            Side.Top => Axis.Y,
            _ => throw new System.ArgumentException("invalid side")
        };

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
            Side.Top => Side.Bottom,
            Side.Bottom => Side.Top,
            _ => side
        };    
}