using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Rooms;

public class OilTankRoom : Room
{
    private OilTank _oilTank;

    public OilTankRoom()
    {
        MainTexture = new TextureInfo(Key: TextureKey.Ceiling, Color: Color.DarkGray);
        _oilTank = new OilTank(this);
    }

    override protected void BeforeBuild()
    {
        _oilTank.Position = Position;
        this.Place().OnFloor();
    }
}
