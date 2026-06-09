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
        room.LocalPosition = Vector3.Zero;

        var decalStamp = new WallDecalStamp();
        worldSegment.AddChild(decalStamp);

        // Create wall decals on each wall using center UV positions
        // (0,0) = center of wall, positive U = right, positive V = up
        
        // North wall - decal at UV (0, 0.5) - center horizontally, slightly up
        var northDecal = new WallDecal(room, Side.North, new Vector2(0, 0.5f));
        northDecal.Width = 2f;
        northDecal.Height = 1f;
        room.AddChild(northDecal);

        // South wall - decal at UV (-1, 1) - left side, upper region
        var southDecal = new WallDecal(room, Side.South, new Vector2(-1, 1));
        southDecal.Width = 1.5f;
        southDecal.Height = 1.5f;
        room.AddChild(southDecal);

        // East wall - decal at UV (0, -0.5) - center horizontally, lower region
        var eastDecal = new WallDecal(room, Side.East, new Vector2(0, -0.5f));
        eastDecal.Width = 2f;
        eastDecal.Height = 2f;
        room.AddChild(eastDecal);

        // West wall - decal at UV (1.5, 0) - right side, centered vertically
        var westDecal = new WallDecal(room, Side.West, new Vector2(1.5f, 0));
        westDecal.Width = 3f;
        westDecal.Height = 1f;
        room.AddChild(westDecal);

        return worldSegment;
    }
}
