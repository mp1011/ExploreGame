using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Tests.WallDecalPlacement;

/// <summary>
/// Test world with a single room that has a gap in the north wall
/// </summary>
public class WallWithGapWorldSegment : WorldSegment
{
    public Room MainRoom { get; }
    public Room ConnectedRoom { get; }
    public float GapStartX { get; }
    public float GapEndX { get; }
    public WallDecalTestShape TestShape { get; }

    public override Vector3 DefaultPlayerStart => new Vector3(0, 0f, 0);

    public override Theme Theme => new UpstairsHallTheme();

    public WallWithGapWorldSegment()
    {
        // Main room: 10x3x10
        MainRoom = new Room(this, width: 10f, height: 3f, depth: 10f, theme: Theme);
        MainRoom.Position = Vector3.Zero;
        MainRoom.Tag = "MainRoom";

        // Small connected room on north side (creates gap)
        ConnectedRoom = new Room(this, width: 2f, height: 3f, depth: 2f, theme: Theme);
        ConnectedRoom.Tag = "ConnectedRoom";

        // Connect rooms - this creates a gap in the north wall
        MainRoom.AddConnectingRoom(ConnectedRoom, Side.North, placement: 0.5f);

        // Calculate gap boundaries (where the connection is)
        // The connection is centered at 50% of the wall (placement: 0.5f)
        // ConnectedRoom is 2 units wide
        var roomWestEdge = MainRoom.GetSide(Side.West);
        var roomWidth = MainRoom.Width;
        var centerX = roomWestEdge + (roomWidth * 0.5f);
        
        GapStartX = centerX - 1.0f; // Half of ConnectedRoom width
        GapEndX = centerX + 1.0f;   // Half of ConnectedRoom width

        AddChild(new WallDecalStamp());

        // Add test shape that will host the controller
        TestShape = new WallDecalTestShape(this);
        TestShape.Position = Vector3.Zero;
        AddChild(TestShape);
    }
}
