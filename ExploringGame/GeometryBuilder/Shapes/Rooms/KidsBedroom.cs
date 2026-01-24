using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class KidsBedroom : Room
{
    private UpstairsHall _upstairsHall;

    public KidsBedroom(WorldSegment worldSegment, UpstairsHall upstairsHall) : base(worldSegment)
    {
        _upstairsHall = upstairsHall;
        Width = Measure.Feet(16);
        Height = Measure.Feet(7);
        Depth = Measure.Feet(16);

        this.Place().OnSideInner(Side.SouthWest);
        this.X += Measure.Feet(17);
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.South, HAlign.Left, DoorDirection.Pull, StateKey.KidsBedroomDoorOpen), this, Side.South, HAlign.Left);

        SetSideUnanchored(Side.West, GetSide(Side.West) + 1.5f);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
