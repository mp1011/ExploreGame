using ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class GarageDoor : Room
{
    public override Theme Theme => new BasementRoomTheme();

    public GarageDoor(WorldSegment worldSegment, Garage garage, Driveway driveway, float zOffset) : base(worldSegment)
    {
        Depth = Measure.Feet(10);
        Height = garage.Height;
        Width = Measure.Feet(1);

        Position = garage.Position;

        garage.AddConnectingRoomWithJunction(this, driveway, Side.West, HAlign.Center, adjustPlacement: false);

        Z += zOffset;
    }
}
