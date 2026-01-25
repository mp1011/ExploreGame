using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class KidsBedroom : Room
{
    private UpstairsHall _upstairsHall;

    public KidsBedroom(WorldSegment worldSegment, UpstairsHall upstairsHall, Bedroom bedroom)
        : base(worldSegment, width: Measure.Feet(12), depth: Measure.Feet(12), height: Measure.Feet(7))
    {
        _upstairsHall = upstairsHall;
        this.Place().OnSideInner(Side.SouthWest)
            .OnSideOuter(Side.East, bedroom, 0.25f);
         
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.South, HAlign.Left, DoorDirection.Pull, StateKey.KidsBedroomDoorOpen), this, Side.South, HAlign.Left,
            adjustPlacement: false);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
