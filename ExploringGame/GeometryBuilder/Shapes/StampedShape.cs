using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Rendering;
using System;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder;


public abstract class StampedShape : PlaceableShape
{
    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        // StampedShapes don't generate their own geometry
        // They use the geometry from their ShapeStamp template
        return Array.Empty<Triangle>();
    }

    public abstract ShapeBuffer GetStampBuffer(Dictionary<Type, ShapeBuffer> stampBuffers);
}

/// <summary>
/// A shape that uses the geometry from a ShapeStamp template.
/// Can be created at runtime without generating new vertex buffers.
/// </summary>
public abstract class StampedShape<TStamp> : StampedShape
    where TStamp : ShapeStamp
{
    
    public override ShapeBuffer GetStampBuffer(Dictionary<Type, ShapeBuffer> stampBuffers)
    {
        if (!stampBuffers.TryGetValue(typeof(TStamp), out var buffer))
            throw new Exception($"No ShapeBuffer found for stamp type {typeof(TStamp).Name}");
        
        return buffer;
    }
}
