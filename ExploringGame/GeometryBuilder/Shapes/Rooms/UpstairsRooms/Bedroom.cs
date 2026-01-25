using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class Bedroom : Room
{
    private UpstairsHall _upstairsHall;

    public Bedroom(WorldSegment worldSegment, UpstairsHall upstairsHall) 
        : base(worldSegment, width: Measure.Feet(17 ), depth: Measure.Feet(12), height: Measure.Feet(7))
    {
        _upstairsHall = upstairsHall;
        this.Place().OnSideInner(Side.SouthWest);
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(this, Side.South, HAlign.Left, DoorDirection.Pull,  StateKey.BedroomDoorOpen), this, Side.South, HAlign.Right, 
                adjustPlacement: false);
    }

    public override Theme Theme =>  new UpstairsHallTheme();
}
