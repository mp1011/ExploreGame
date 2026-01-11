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
        _shape.Position = other.Position;
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
                _shape.SetSideUnanchored(Side.West, _shape.GetSide(Side.West) + amount);
                return this;
            case Side.North:
                _shape.SetSideUnanchored(Side.East, _shape.GetSide(Side.East) - amount);
                return this;
            case Side.West:
                _shape.SetSideUnanchored(Side.North, _shape.GetSide(Side.North) + amount);
                return this;
            case Side.East:
                _shape.SetSideUnanchored(Side.South, _shape.GetSide(Side.South) - amount);
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
                _shape.SetSideUnanchored(Side.East, _shape.GetSide(Side.East) - amount);
                return this;
            case Side.North:
                _shape.SetSideUnanchored(Side.West, _shape.GetSide(Side.West) + amount);
                return this;
            case Side.West:
                _shape.SetSideUnanchored(Side.South, _shape.GetSide(Side.South) - amount);
                return this;
            case Side.East:
                _shape.SetSideUnanchored(Side.North, _shape.GetSide(Side.North) + amount);
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
                _shape.SetSideUnanchored(Side.Top, _shape.GetSide(Side.Top) - amount);
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
                _shape.SetSideUnanchored(Side.Bottom, _shape.GetSide(Side.Bottom) + amount);
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
        _shape.TopAnchored = _shape.TopAnchored - fromTop;
        _shape.BottomUnanchored = _shape.TopAnchored - height;
        return this;
    }

    public ShapeAdjuster SliceFromWest(float fromWest, float width)
    {
        _shape.SetSide(Side.West, _shape.GetSide(Side.West) + fromWest);
        _shape.SetSideUnanchored(Side.East, _shape.GetSide(Side.West) + width);
        return this;
    }

    public ShapeAdjuster SliceFromNorth(float fromNorth, float depth)
    {
        _shape.SetSide(Side.North, _shape.GetSide(Side.North) + fromNorth);
        _shape.SetSideUnanchored(Side.South, _shape.GetSide(Side.North) + depth);
        return this;
    }

    public ShapeAdjuster SliceFromBottom(float fromBottom, float height)
    {
        _shape.SetSide(Side.Bottom, _shape.GetSide(Side.Bottom) + fromBottom);
        _shape.SetSideUnanchored(Side.Top, _shape.GetSide(Side.Bottom) + height);
        return this;
    }

    public ShapeAdjuster SliceFromEast(float fromEast, float width)
    {
        _shape.SetSide(Side.East, _shape.GetSide(Side.East) - fromEast);
        _shape.SetSideUnanchored(Side.West, _shape.GetSide(Side.East) - width);
        return this;
    }

    public ShapeAdjuster SliceFromSouth(float fromSouth, float depth)
    {
        _shape.SetSide(Side.South, _shape.GetSide(Side.South) - fromSouth);
        _shape.SetSideUnanchored(Side.North, _shape.GetSide(Side.South) - depth);
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
}
