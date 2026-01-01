using System;

namespace ExploringGame.GeometryBuilder;

public class WorldSegment : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public WorldSegment(params Shape[] contents)
    {
        foreach (var item in contents)
            AddChild(item);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
