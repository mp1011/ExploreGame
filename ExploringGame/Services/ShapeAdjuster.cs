using ExploringGame.Entities;
using ExploringGame.GeometryBuilder;

namespace ExploringGame.Services;

public static partial class ShapeExtensions
{
    public static ShapeAdjuster AdjustShape(this Shape shape)
    {
        return new ShapeAdjuster(shape);
    }
}
public class ShapeAdjuster
{
    private Shape _shape;

    public ShapeAdjuster(Shape shape)
    {
        _shape = shape;
    }

    public ShapeAdjuster From(Shape other)
    {
        _shape.Size = other.Size;
        _shape.WorldPosition = other.WorldPosition;
        return this;
    }

    public ShapeAdjuster AddToSide(Side side, float amount)
    {
        _shape.SetWorldSideUnanchored(side, _shape.GetWorldSide(side) + amount);
        return this;
    }

    public ShapeAdjuster WithInnerOffset(Placement2D placement, Side face)
    {
        AddSideLeft(face, placement.Left);
        AddSideRight(face, placement.Right);
        AddSideTop(face, placement.Top);
        AddSideBottom(face, placement.Bottom);
        return this;
    }

    public ShapeAdjuster AddSideLeft(Side face, float amount)
    {
        switch(face)
        {
            case Side.South:
                _shape.SetWorldSideUnanchored(Side.West, _shape.GetWorldSide(Side.West) + amount);
                return this;
            case Side.North:
                _shape.SetWorldSideUnanchored(Side.East, _shape.GetWorldSide(Side.East) - amount);
                return this;
            case Side.West:
                _shape.SetWorldSideUnanchored(Side.North, _shape.GetWorldSide(Side.North) + amount);
                return this;
            case Side.East:
                _shape.SetWorldSideUnanchored(Side.South, _shape.GetWorldSide(Side.South) - amount);
                return this;
            case Side.Bottom:
            case Side.Top:
                throw new System.Exception("fix me");
            default:
                throw new System.ArgumentException("invalid side");
        }
    }

    public ShapeAdjuster AddSideRight(Side face, float amount)
    {
        switch (face)
        {
            case Side.South:
                _shape.SetWorldSideUnanchored(Side.East, _shape.GetWorldSide(Side.East) - amount);
                return this;
            case Side.North:
                _shape.SetWorldSideUnanchored(Side.West, _shape.GetWorldSide(Side.West) + amount);
                return this;
            case Side.West:
                _shape.SetWorldSideUnanchored(Side.South, _shape.GetWorldSide(Side.South) - amount);
                return this;
            case Side.East:
                _shape.SetWorldSideUnanchored(Side.North, _shape.GetWorldSide(Side.North) + amount);
                return this;
            case Side.Bottom:
            case Side.Top:
                throw new System.Exception("fix me");
            default:
                throw new System.ArgumentException("invalid side");
        }
    }

    public ShapeAdjuster AddSideTop(Side face, float amount)
    {
        switch (face)
        {
            case Side.South:
            case Side.North:
            case Side.West:
            case Side.East:
                _shape.SetWorldSideUnanchored(Side.Top, _shape.GetWorldSide(Side.Top) - amount);
                return this;
            case Side.Bottom:
            case Side.Top:
                throw new System.Exception("fix me");
            default:
                throw new System.ArgumentException("invalid side");
        }
    }

    public ShapeAdjuster AddSideBottom(Side face, float amount)
    {
        switch (face)
        {
            case Side.South:
            case Side.North:
            case Side.West:
            case Side.East:
                _shape.SetWorldSideUnanchored(Side.Bottom, _shape.GetWorldSide(Side.Bottom) + amount);
                return this;
            case Side.Bottom:
            case Side.Top:
                throw new System.Exception("fix me");
            default:
                throw new System.ArgumentException("invalid side");
        }
    }

    public ShapeAdjuster SliceFromTop(float fromTop, float height)
    {
        _shape.SetWorldSide(Side.Top, _shape.GetWorldSide(Side.Top) - fromTop);
        _shape.SetWorldSideUnanchored(Side.Bottom, _shape.GetWorldSide(Side.Top) - height);
        return this;
    }

    public ShapeAdjuster SliceFromWest(float fromWest, float width)
    {
        _shape.SetWorldSide(Side.West, _shape.GetWorldSide(Side.West) + fromWest);
        _shape.SetWorldSideUnanchored(Side.East, _shape.GetWorldSide(Side.West) + width);
        return this;
    }

    public ShapeAdjuster SliceFromNorth(float fromNorth, float depth)
    {
        _shape.SetWorldSide(Side.North, _shape.GetWorldSide(Side.North) + fromNorth);
        _shape.SetWorldSideUnanchored(Side.South, _shape.GetWorldSide(Side.North) + depth);
        return this;
    }

    public ShapeAdjuster SliceFromBottom(float fromBottom, float height)
    {
        _shape.SetWorldSide(Side.Bottom, _shape.GetWorldSide(Side.Bottom) + fromBottom);
        _shape.SetWorldSideUnanchored(Side.Top, _shape.GetWorldSide(Side.Bottom) + height);
        return this;
    }

    public ShapeAdjuster SliceFromEast(float fromEast, float width)
    {
        _shape.SetWorldSide(Side.East, _shape.GetWorldSide(Side.East) - fromEast);
        _shape.SetWorldSideUnanchored(Side.West, _shape.GetWorldSide(Side.East) - width);
        return this;
    }

    public ShapeAdjuster SliceFromSouth(float fromSouth, float depth)
    {
        _shape.SetWorldSide(Side.South, _shape.GetWorldSide(Side.South) - fromSouth);
        _shape.SetWorldSideUnanchored(Side.North, _shape.GetWorldSide(Side.South) - depth);
        return this;
    }


    /// <summary>
    /// Adds an amount to each side, preserving the center
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public ShapeAdjuster AxisStretch(Axis axis, float add)
    {
        if(axis.HasFlag(Axis.X))
        {
            _shape.Width += add;
        }

        if (axis.HasFlag(Axis.Y))
        {
            _shape.Height += add;
        }

        if (axis.HasFlag(Axis.Z))
        {
            _shape.Depth += add;
        }

        return this;
    }

    /// <summary>
    /// Sets the given axis size, preserving the center
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public ShapeAdjuster SetAxis(Axis axis, float value)
    {
        if (axis.HasFlag(Axis.X))
        {
            _shape.Width = value;
        }

        if (axis.HasFlag(Axis.Y))
        {
            _shape.Height = value;
        }

        if (axis.HasFlag(Axis.Z))
        {
            _shape.Depth = value;
        }

        return this;
    }

    /// <summary>
    /// Sets the given axis size, preserving the center
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public ShapeAdjuster SetAxis(Axis axis, float from, float to)
    {
        if (axis.HasFlag(Axis.X))
        {
            _shape.SetWorldSide(Side.West, from);
            _shape.SetWorldSideUnanchored(Side.East, to);
        }

        if (axis.HasFlag(Axis.Y))
        {
            _shape.SetWorldSide(Side.Bottom, from);
            _shape.SetWorldSideUnanchored(Side.Top, to);
        }

        if (axis.HasFlag(Axis.Z))
        {
            _shape.SetWorldSide(Side.North, from);
            _shape.SetWorldSideUnanchored(Side.South, to);
        }

        return this;
    }
}
