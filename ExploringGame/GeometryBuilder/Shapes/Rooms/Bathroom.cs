using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class Bathroom : Room
{
    private readonly UpstairsHall _upstairsHall;
    public override Theme Theme => new BathroomTheme();

    public Bathroom(UpstairsWorldSegment worldSegment, UpstairsHall hall) : base(worldSegment)
    {
        _upstairsHall = hall;
        Height = hall.Height;
        Width = Measure.Feet(10);
        Depth = Measure.Feet(5);
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.East, HAlign.Right, DoorDirection.Pull, StateKey.BathroomDoorOpen), this, Side.East);
    }
}
