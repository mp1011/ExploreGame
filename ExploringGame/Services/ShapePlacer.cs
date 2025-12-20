using ExploringGame.GeometryBuilder;

namespace ExploringGame.Services;

public static class ShapeExtensions
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

    public void OnFloor()
    {
        _shape.BottomAnchored = _shape.Parent.BottomAnchored;
    }

    public void OnSide(Side side)
    {
        foreach(var s in side.Decompose())
        {
            _shape.SetSide(s, _shape.Parent.GetSide(s));
        }
            
    }
}
