using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;

namespace ExploringGame.Tests.TestHelpers;

public class TestWorldSegment : WorldSegment
{
    public override Vector3 DefaultPlayerStart => PlayerStart;

    public Vector3 PlayerStart { get; set; }

    public TestWorldSegment(Vector3? playerStart = null)
    {
        if (playerStart.HasValue)
            PlayerStart = playerStart.Value;
    }
}
