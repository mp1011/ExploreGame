using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;

public class LivingRoom : Room 
{
    private readonly UpstairsHall _upstairsHall;
    private readonly Kitchen _kitchen;

    public override Theme Theme { get; }

    public LivingRoom(WorldSegment segment, UpstairsHall upstairs, Kitchen kitchen) 
        : base(segment, height: Measure.Feet(7), depth: Measure.Feet(17), width: Measure.Feet(24))
    {
        _upstairsHall = upstairs;
        _kitchen = kitchen;


        Theme = new UpstairsHallTheme();
        Theme.SideTextures[Side.North] = new TextureInfo(TextureKey.Wood);

        this.Place().OnSideInner(Side.NorthWest);
    }

    public override void LoadChildren()
    { 
        AddConnectingRoom(new RoomConnection(this, _kitchen, Side.South), adjustPlacement: false);
        AddConnectingRoom(new RoomConnection(this, _upstairsHall.NorthHall, Side.South), adjustPlacement: false);
    }
}
