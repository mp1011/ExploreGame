using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.LevelControl;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class HalfBathroom : Room
{
    private readonly Den _den;
    public override Theme Theme => new BathroomTheme();

    public HalfBathroom(UpstairsWorldSegment worldSegment, Den den) : base(worldSegment)
    {
        _den = den;
        Height = _den.Height;
        Width = Measure.Feet(5);
        Depth = Measure.Feet(5);
    }

    public override void LoadChildren()
    {
        _den.EastPart.AddConnectingRoomWithJunction(new DoorJunction(this, Side.North, HAlign.Left, DoorDirection.Pull, StateKey.HalfBathroomDoorOpen),
            this, Side.North);
    }
}
