using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class SpareRoom : Room
{
    private UpstairsHall _upstairsHall;

    public SpareRoom(WorldSegment worldSegment, UpstairsHall upstairsHall, Bedroom bedroom) 
        : base(worldSegment, height: upstairsHall.Height, width: Measure.Feet(12), depth: Measure.Feet(16))
    {
        _upstairsHall = upstairsHall;
        this.Place().OnSideInner(Side.West);
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(_upstairsHall.SouthHall, Side.West, HAlign.Left, DoorDirection.Pull, StateKey.SpareRoomDoorOpen),        
            this, Side.West, HAlign.Left, 3.0f, adjustPlacement: false);
    }

    public override Theme Theme => new UpstairsHallTheme();
}
