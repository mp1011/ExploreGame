using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Decals;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Testing;

public static partial class TestMaps
{
    public static WorldSegment WallDecalTest()
    {
        var worldSegment = new TestWorldSegment();
        worldSegment.PlayerStart = new Vector3(0, 1.5f, 5);

        // Create a simple room
        var room = new Room(worldSegment, theme: new BasementRoomTheme());
        room.Width = 10f;
        room.Height = 7f;
        room.Depth = 10f;
        room.Position = Vector3.Zero;

        var decalStamp = new WallDecalStamp();
        worldSegment.AddChild(decalStamp);

        // Create wall decals on each wall (using Left, Top, Right, Bottom)
        // North wall (facing into room from -Z)
        var northDecal = new WallDecal(room, Side.North, new Placement2D(1, 1.5f, 3, 0.5f)); // 2 wide, 1 tall
        room.AddChild(northDecal);

        // South wall
        var southDecal = new WallDecal(room, Side.South, new Placement2D(2, 2.5f, 3.5f, 1.0f)); // 1.5 wide, 1.5 tall
        room.AddChild(southDecal);

        // East wall
        var eastDecal = new WallDecal(room, Side.East, new Placement2D(3, 2.5f, 5, 0.5f)); // 2 wide, 2 tall
        room.AddChild(eastDecal);

        // West wall
        var westDecal = new WallDecal(room, Side.West, new Placement2D(1, 2.5f, 4, 1.5f)); // 3 wide, 1 tall
        room.AddChild(westDecal);

        return worldSegment;
    }
}
