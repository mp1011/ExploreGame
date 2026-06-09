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
        MainRoom.LocalPosition = Vector3.Zero;
        MainRoom.Tag = "MainRoom";

        // Small connected room (creates gap) - 2x3x2
        ConnectedRoom = new Room(this, width: 2f, height: 3f, depth: 2f, theme: Theme);
        ConnectedRoom.Tag = "ConnectedRoom";

        // Position gap near the "right" edge (leaves minimal space on right side)
        // For North/South: right = East, left = West
        // For East/West: right = South, left = North
        MainRoom.AddConnectingRoom(new RoomConnection(MainRoom, ConnectedRoom, wallSide, HAlign.Right, -0.3f));

        // Calculate gap boundaries based on wall orientation
        // NOTE: Room.AddConnectingRoom inverts position for South/West walls (line 82-83 in Room.cs)
        // So HAlign.Right actually places the gap on the LEFT side for South/West!
        var (axisU, _) = wallSide.GetAxisUV();

        if (wallSide == Side.North)
        {
            // North: right = East
            var roomEastEdge = MainRoom.GetSide(Side.East);
            GapStartX = roomEastEdge - 0.3f - 2f;
            GapEndX = roomEastEdge - 0.3f;
        }
        else if (wallSide == Side.South)
        {
            // South: position inverted, so HAlign.Right puts gap on West (left in 2D space)
            var roomWestEdge = MainRoom.GetSide(Side.West);
            GapStartX = roomWestEdge + 0.3f;
            GapEndX = roomWestEdge + 0.3f + 2f;
        }
        else if (wallSide == Side.East)
        {
            // East: right = South
            var roomSouthEdge = MainRoom.GetSide(Side.South);
            GapStartX = roomSouthEdge - 0.3f - 2f;
            GapEndX = roomSouthEdge - 0.3f;
        }
        else // West
        {
            // West: position inverted, so HAlign.Right puts gap on North (left in 2D space)
            var roomNorthEdge = MainRoom.GetSide(Side.North);
            GapStartX = roomNorthEdge + 0.3f;
            GapEndX = roomNorthEdge + 0.3f + 2f;
        }

        AddChild(new WallDecalStamp());

        // Add test shape that will host the controller
        TestShape = new WallDecalTestShape(this);
        TestShape.LocalPosition = Vector3.Zero;
        AddChild(TestShape);
    }
}

