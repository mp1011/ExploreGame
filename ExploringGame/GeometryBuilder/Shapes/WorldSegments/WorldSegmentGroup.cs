using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public abstract class WorldSegmentGroup
{
    public abstract IEnumerable<WorldSegment> CreateSegments();

    public virtual Vector3 DefaultPlayerStart { get; }
}

public class HomeWorldSegmentGroup : WorldSegmentGroup
{
    public override Vector3 DefaultPlayerStart => OutsideWorldSegment.DefaultPlayerStart;
    public override IEnumerable<WorldSegment> CreateSegments()
    {
        yield return new BasementWorldSegment();
        yield return new UpstairsWorldSegment();
        yield return new OutsideWorldSegment();
        yield return new BackyardWorldSegment();
        yield return new NeighborhoodWorldSegment();
    }
}

public class SingleSegmentGroup : WorldSegmentGroup
{
    private WorldSegment _segment;

    public override Vector3 DefaultPlayerStart => _segment.DefaultPlayerStart;

    public SingleSegmentGroup(WorldSegment segment)
    {
        _segment = segment;
    }

    public override IEnumerable<WorldSegment> CreateSegments()
    {
        yield return _segment;
    }
}