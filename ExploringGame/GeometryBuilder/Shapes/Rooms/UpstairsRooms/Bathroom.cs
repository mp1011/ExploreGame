using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class Bathroom : Room
{
    private readonly UpstairsHall _upstairsHall;
    public override Theme Theme => new BathroomTheme();

    public Bathroom(UpstairsWorldSegment worldSegment, UpstairsHall hall) 
        : base(worldSegment, height: hall.Height, width: Measure.Feet(10), depth: Measure.Feet(5))
    {
        _upstairsHall = hall;
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.East, HAlign.Right, DoorDirection.Pull, StateKey.BathroomDoorOpen), this, Side.East);
    }
}
