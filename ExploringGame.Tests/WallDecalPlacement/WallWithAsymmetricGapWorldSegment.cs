using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Tests.WallDecalPlacement;

/// <summary>
/// Test world with a gap positioned closer to the east side, making the east quad too small for decals
/// </summary>
public class WallWithAsymmetricGapWorldSegment : WorldSegment, IGapWorldSegment
{
    public Room MainRoom { get; }
    public Room ConnectedRoom { get; }
    public float GapStartX { get; }
    public float GapEndX { get; }
    public WallDecalTestShape TestShape { get; }

    public override Vector3 DefaultPlayerStart => new Vector3(0, 0f, 0);

    public override Theme Theme => new UpstairsHallTheme();

    public WallWithAsymmetricGapWorldSegment()
    {
        // Main room: 10x3x10
        MainRoom = new Room(this, width: 10f, height: 3f, depth: 10f, theme: Theme);
        MainRoom.Position = Vector3.Zero;
        MainRoom.Tag = "MainRoom";

        // Small connected room on north side (creates gap)
        ConnectedRoom = new Room(this, width: 2f, height: 3f, depth: 2f, theme: Theme);
        ConnectedRoom.Tag = "ConnectedRoom";

        // this will leave the east side with not enough space for a decal
        MainRoom.AddConnectingRoom(new RoomConnection(MainRoom, ConnectedRoom, Side.North, HAlign.Right, -0.3f));

        var roomEastEdge = MainRoom.GetSide(Side.East);
        GapStartX = roomEastEdge - 0.3f - 2f;
        GapEndX = roomEastEdge - 0.3f;

        AddChild(new WallDecalStamp());

        // Add test shape that will host the controller
        TestShape = new WallDecalTestShape(this);
        TestShape.Position = Vector3.Zero;
        AddChild(TestShape);
    }
}
