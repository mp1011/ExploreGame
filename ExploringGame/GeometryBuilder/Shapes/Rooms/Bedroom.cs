using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class Bedroom : Room
{
    private UpstairsHall _upstairsHall;

    public Bedroom(WorldSegment worldSegment, UpstairsHall upstairsHall) : base(worldSegment)
    {
        _upstairsHall = upstairsHall;
        Width = Measure.Feet(10);
        Height = Measure.Feet(8);
        Depth = Measure.Feet(10);
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.South, HAlign.Left, DoorDirection.Pull,  StateKey.BedroomDoorOpen), this, Side.South, HAlign.Right);

        SetSideUnanchored(Side.East, GetSide(Side.East) - 1.5f);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
