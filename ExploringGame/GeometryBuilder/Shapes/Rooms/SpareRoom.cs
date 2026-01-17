using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class SpareRoom : Room
{
    private UpstairsHall _upstairsHall;

    public SpareRoom(WorldSegment worldSegment, UpstairsHall upstairsHall) : base(worldSegment)
    {
        _upstairsHall = upstairsHall;
        Width = Measure.Feet(10);
        Height = Measure.Feet(8);
        Depth = Measure.Feet(10);
    }

    public override void LoadChildren()
    {
        _upstairsHall.SouthHall.AddConnectingRoomWithJunction(
            new DoorJunction(_upstairsHall.SouthHall, Side.West, HAlign.Left, StateKey.SpareRoomDoorOpen),        
            this, Side.West, HAlign.Left, 3.0f);

        SetSideUnanchored(Side.South, GetSide(Side.South) - 1.5f);
    }

    public override Theme Theme => new UpstairsHallTheme();
}
