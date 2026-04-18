using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;

namespace ExploringGame.Testing;

public class TestWorldSegment : WorldSegment
{
    public Vector3 DefaultPlayerStart => PlayerStart;

    public Vector3 PlayerStart { get; set; }

    public TestWorldSegment(Vector3? playerStart = null)
    {
        if (playerStart.HasValue)
            PlayerStart = playerStart.Value;
    }
}

public class TestWorldSegmentA : TestWorldSegment
{
    public TestWorldSegmentA() : base() { }
}

public class TestWorldSegmentB : TestWorldSegment
{
    public TestWorldSegmentB() : base() { }
}

public class TestWorldSegmentC : TestWorldSegment
{
    public TestWorldSegmentC() : base() { }
}

public class TestWorldSegmentD : TestWorldSegment
{
    public TestWorldSegmentD() : base() { }
}

public class TestWorldSegmentE : TestWorldSegment
{
    public TestWorldSegmentE() : base() { }
}
