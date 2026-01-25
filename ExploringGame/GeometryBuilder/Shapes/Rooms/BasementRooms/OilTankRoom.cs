using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms.BasementRooms;

public class OilTankRoom : Room
{
    public override Theme Theme => new Theme { MainTexture = new TextureInfo(TextureKey.Ceiling) };
    private OilTank _oilTank;

    public OilTankRoom(WorldSegment worldSegment) : base(worldSegment)
    {
        _oilTank = new OilTank(this);
    }

    override protected void BeforeBuild()
    {
        _oilTank.Position = Position;
        this.Place().OnFloor();
    }
}
