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
            new DoorJunction(_worldSegment, doorClose: new Angle(Side.East), doorOpen: new Angle(Side.South), HAlign.Right, StateKey.KidsBedroomDoorOpen,
            width: Measure.Inches(30.5f), depth: 0.2f, height: Height), this, Side.South, HAlign.Left);

        SetSideUnanchored(Side.West, GetSide(Side.West) + 1.5f);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
