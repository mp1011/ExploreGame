using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class LivingRoom : Room 
{
    private readonly UpstairsHall _upstairsHall;
    private readonly Kitchen _kitchen;

    public override Theme Theme { get; }

    public LivingRoom(WorldSegment segment, UpstairsHall upstairs, Kitchen kitchen) : base(segment)
    {
        _upstairsHall = upstairs;
        _kitchen = kitchen;

        Height = Measure.Feet(7);
        Width = Measure.Feet(0);
        Depth = Measure.Feet(0);

        Theme = new UpstairsHallTheme();
        Theme.SideTextures[Side.North] = new TextureInfo(TextureKey.Wood);

        this.Place().OnSideInner(Side.NorthWest);
    }

    public override void LoadChildren()
    {
        SetSide(Side.Bottom, _kitchen.GetSide(Side.Bottom));
        SetSide(Side.South, _kitchen.GetSide(Side.North));
        SetSide(Side.East, _kitchen.GetSide(Side.East));

        AddConnectingRoom(new RoomConnection(this, _kitchen, Side.South), adjustPlacement: false);
        AddConnectingRoom(new RoomConnection(this, _upstairsHall.NorthHall, Side.South), adjustPlacement: false);
    }
}
