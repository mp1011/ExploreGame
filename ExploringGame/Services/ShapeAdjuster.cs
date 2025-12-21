using ExploringGame.GeometryBuilder;

namespace ExploringGame.Services;

public static partial class ShapeExtensions
{
    public static ShapeAdjuster Adjust(this Shape shape)
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
        _shape.Rotation = other.Rotation;
        return this;
    }

    public ShapeAdjuster SliceY(float fromTop, float height)
    {
        _shape.TopAnchored = _shape.Parent.TopAnchored - fromTop;
        _shape.BottomUnanchored = _shape.TopAnchored - height;
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
            _shape.X -= add / 2f;
            _shape.Width += add;
        }

        if (axis.HasFlag(Axis.Y))
        {
            _shape.Y -= add / 2f;
            _shape.Height += add;
        }

        if (axis.HasFlag(Axis.Z))
        {
            _shape.Z -= add / 2f;
            _shape.Depth += add;
        }

        return this;
    }
}
