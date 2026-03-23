using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes;


public abstract class PlaceholderShape : Room
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public PlaceholderShape(WorldSegment worldSegment) : base(worldSegment)
    {

    }
}

// non-rendered shape that acts as a placeholder for a shape in another WorldSegment
public class PlaceholderShape<T> : PlaceholderShape
    where T : Shape
{
    public PlaceholderShape(WorldSegment worldSegment, string tag, Vector3 position, Vector3 size) : base(worldSegment)
    {
        Position = position;
        Size = size;
        Tag = tag;
    }

    public PlaceholderShape(WorldSegment worldSegment, Vector3 position, Vector3 size) : base(worldSegment)
    {
        Position = position;
        Size = size;
    }
}
