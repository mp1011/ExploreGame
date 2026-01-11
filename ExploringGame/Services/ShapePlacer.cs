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

    public ShapePlacer FromNorth(float amount) => FromSide(Side.North, amount);
    public ShapePlacer FromSouth(float amount) => FromSide(Side.South, amount);
    public ShapePlacer FromEast(float amount) => FromSide(Side.East, amount);
    public ShapePlacer FromWest(float amount) => FromSide(Side.West, amount);

    public ShapePlacer FromSide(Side side, float amount)
    {
        if(side == Side.South || side == Side.East || side == Side.Top)
            amount = -amount;

        _shape.SetSide(side, _shape.Parent.GetSide(side) + amount);
        return this;
    }
}
