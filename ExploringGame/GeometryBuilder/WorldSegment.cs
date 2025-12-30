using System;

namespace ExploringGame.GeometryBuilder;

public class WorldSegment : Shape
{
    public override ViewFrom ViewFrom => ViewFrom.None;
    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }
}
