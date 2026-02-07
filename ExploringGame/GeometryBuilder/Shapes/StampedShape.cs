using ExploringGame.GeometryBuilder.Shapes;
using System;

namespace ExploringGame.GeometryBuilder;

/// <summary>
/// A shape that uses the geometry from a ShapeStamp template.
/// Can be created at runtime without generating new vertex buffers.
/// </summary>
public abstract class StampedShape<TStamp> : PlaceableShape
    where TStamp : ShapeStamp
{
    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        // StampedShapes don't generate their own geometry
        // They use the geometry from their ShapeStamp template
        return Array.Empty<Triangle>();
    }
}
