using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Rooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Testing;

public static partial class TestMaps
{
    public static WorldSegment ShapeStampTest()
    {
        var worldSegment = new TestWorldSegment();
        worldSegment.PlayerStart = new Vector3(0, 0, 5);

        // Create a simple room
        var room = new Room(worldSegment, theme: new UpstairsHallTheme());
        room.Width = 10f;
        room.Height = 6f;
        room.Depth = 10f;
        room.Position = Vector3.Zero;

        // Add the ShapeStamp template (will be built but not rendered)
        var shapeStamp = new TestShapeStamp();
        worldSegment.AddChild(shapeStamp);

        // Add the generator
        var generator = room.AddChild(new TestShapeStampGenerator());
        generator.Place().OnFloor();
        generator.Y += 2f;
        
        return worldSegment;
    }
}