using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class HalfBathroom : Room
{
    private readonly Den _den;
    public override Theme Theme => new BathroomTheme();

    public HalfBathroom(UpstairsWorldSegment worldSegment, Den den) 
        : base(worldSegment, height: den.Height, width: Measure.Feet(5), depth: Measure.Feet(5))
    {
        _den = den;
    }

    public override void LoadChildren()
    {
        _den.EastPart.AddConnectingRoomWithJunction(new DoorJunction(this, Side.North, HAlign.Left, DoorDirection.Pull, StateKey.HalfBathroomDoorOpen),
            this, Side.North);
    }
}
