using ExploringGame.GeometryBuilder;

namespace ExploringGame.Services;

public static partial class ShapeExtensions
{
    public static ShapePlacer Place(this Shape shape)
    {
        return new ShapePlacer(shape);
    }
}
public class ShapePlacer
{
    private Shape _shape;
    public ShapePlacer(Shape shape)
    {
        _shape = shape;
    }

    public ShapePlacer OnFloor()
    {
        _shape.BottomAnchored = _shape.Parent.BottomAnchored;
        return this;
    }

    public ShapePlacer OnSideInner(Side side, Shape other = null)
    {
        other = other ?? _shape.Parent;
        foreach(var s in side.Decompose())
        {
            _shape.SetSide(s, other.GetSide(s));
        }
        return this;
    }

    public ShapePlacer OnSideOuter(Side side, Shape other = null)
    {
        other = other ?? _shape.Parent;
        foreach (var s in side.Decompose())
        {
            _shape.SetSide(s.Opposite(), other.GetSide(s));
        }
        return this;
    }
}
