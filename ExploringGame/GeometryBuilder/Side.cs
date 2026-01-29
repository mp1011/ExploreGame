using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
    None = 0,
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

    public static (Axis,Axis) GetAxisUV(this Side side) =>
       side switch
       {
           Side.North => (Axis.X, Axis.Y),
           Side.South => (Axis.X, Axis.Y),
           Side.West => (Axis.Z, Axis.Y),
           Side.East => (Axis.Z, Axis.Y),
           Side.Bottom => (Axis.X, Axis.Z),
           Side.Top => (Axis.X, Axis.Z),
           _ => throw new System.ArgumentException("invalid side")
       };

    public static Side[] Decompose(this Side side)
    {
        var sides = new[] { Side.West, Side.North, Side.East, Side.South, Side.Top, Side.Bottom };
        return sides.Where(p=> side.HasFlag(p)).ToArray();
    }

    public static Side CounterClockwiseTurn(this Side side) => new Angle(side).RotateCounterClockwise(90).ToSide();
    public static Side ClockwiseTurn(this Side side) => new Angle(side).RotateClockwise(90).ToSide();

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