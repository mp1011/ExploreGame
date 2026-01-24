using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class Kitchen : Room
{
    private readonly UpstairsHall _upstairsHall;

    public override Theme Theme => new KitchenTheme();
    public Kitchen(WorldSegment worldSegment, UpstairsHall upstairsHall) : base(worldSegment)
    {
        _upstairsHall = upstairsHall;
        Height = upstairsHall.Height;
        Width = Measure.Feet(0);
        Depth = Measure.Feet(0);

    }

    public override void LoadChildren()
    {
        _upstairsHall.AddConnectingRoom(new RoomConnection(_upstairsHall, this, Side.East, HAlign.Right));
        SetSideUnanchored(Side.North, _upstairsHall.NorthHall.GetSide(Side.North));
    }

}
