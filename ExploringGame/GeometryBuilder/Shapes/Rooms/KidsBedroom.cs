using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class KidsBedroom : Room
{
    private UpstairsHall _upstairsHall;

    public KidsBedroom(WorldSegment worldSegment, UpstairsHall upstairsHall) : base(worldSegment)
    {
        _upstairsHall = upstairsHall;
        Width = Measure.Feet(10);
        Height = Measure.Feet(8);
        Depth = Measure.Feet(10);
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.South, HAlign.Left, DoorDirection.Pull, StateKey.KidsBedroomDoorOpen), this, Side.South, HAlign.Left);

        SetSideUnanchored(Side.West, GetSide(Side.West) + 1.5f);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
