using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class WorldSegmentTransition
{
    public Type WorldSegmentType { get; }

    public WorldSegmentTransition(Type worldSegmentType)
    {
        if (!typeof(WorldSegment).IsAssignableFrom(worldSegmentType))
            throw new ArgumentException($"Type must be a WorldSegment type", nameof(worldSegmentType));

        WorldSegmentType = worldSegmentType;
    }
}
