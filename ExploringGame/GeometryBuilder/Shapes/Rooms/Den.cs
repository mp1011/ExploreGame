using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class Den : Room
{
    private LivingRoom _livingRoom;

    public override Theme Theme => new UpstairsHallTheme();

    public Den(UpstairsWorldSegment worldSegment, LivingRoom livingRoom) : base(worldSegment)
    {
        _livingRoom = livingRoom;
        Height = _livingRoom.Height;
        Width = 10f;
        Depth = 10f;
    }

    public override void LoadChildren()
    {
        SetSide(Side.Bottom, _livingRoom.GetSide(Side.Bottom));
        _livingRoom.AddConnectingRoomWithJunction(new DoubleDoorJunction(this, Side.East, DoorDirection.Push, StateKey.DenDoorsOpen), this, Side.East, HAlign.Right, -1.0f);
        SetSideUnanchored(Side.South, _livingRoom.GetSide(Side.South));
    }
}
