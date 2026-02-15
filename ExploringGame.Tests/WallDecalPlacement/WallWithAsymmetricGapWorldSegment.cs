using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Tests.WallDecalPlacement;

/// <summary>
/// Test world with a gap positioned closer to one side, making one quad too small for decals
/// </summary>
public class WallWithAsymmetricGapWorldSegment : WorldSegment, IGapWorldSegment
{
    public Room MainRoom { get; }
    public Room ConnectedRoom { get; }
    public float GapStartX { get; }
    public float GapEndX { get; }
    public Side TestWallSide { get; }
    public WallDecalTestShape TestShape { get; }

    public override Vector3 DefaultPlayerStart => new Vector3(0, 0f, 0);

    public override Theme Theme => new UpstairsHallTheme();

    public WallWithAsymmetricGapWorldSegment(Side wallSide = Side.North)
    {
        TestWallSide = wallSide;

        // Main room: 10x3x10
        MainRoom = new Room(this, width: 10f, height: 3f, depth: 10f, theme: Theme);
        MainRoom.Position = Vector3.Zero;
        MainRoom.Tag = "MainRoom";

        // Small connected room (creates gap) - 2x3x2
        ConnectedRoom = new Room(this, width: 2f, height: 3f, depth: 2f, theme: Theme);
        ConnectedRoom.Tag = "ConnectedRoom";

        // Position gap near the "right" edge (leaves minimal space on right side)
        // For North/South: right = East, left = West
        // For East/West: right = South, left = North
        MainRoom.AddConnectingRoom(new RoomConnection(MainRoom, ConnectedRoom, wallSide, HAlign.Right, -0.3f));

        // Calculate gap boundaries based on wall orientation
        var (axisU, _) = wallSide.GetAxisUV();
        
        if (wallSide == Side.North || wallSide == Side.South)
        {
            // For North/South walls, gap is near East edge (X axis)
            var roomEastEdge = MainRoom.GetSide(Side.East);
            GapStartX = roomEastEdge - 0.3f - 2f; // 2f = ConnectedRoom width
            GapEndX = roomEastEdge - 0.3f;
        }
        else // East or West
        {
            // For East/West walls, gap is near South edge (Z axis)
            var roomSouthEdge = MainRoom.GetSide(Side.South);
            GapStartX = roomSouthEdge - 0.3f - 2f; // 2f = ConnectedRoom depth
            GapEndX = roomSouthEdge - 0.3f;
        }

        AddChild(new WallDecalStamp());

        // Add test shape that will host the controller
        TestShape = new WallDecalTestShape(this);
        TestShape.Position = Vector3.Zero;
        AddChild(TestShape);
    }
}

