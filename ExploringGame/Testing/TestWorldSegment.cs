using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using Microsoft.Xna.Framework;

namespace ExploringGame.Testing;

public class TestWorldSegment : WorldSegment
{
    public override Vector3 DefaultPlayerStart => PlayerStart;

    public Vector3 PlayerStart { get; set; }
}
