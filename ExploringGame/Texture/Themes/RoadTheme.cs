using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class RoadTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public RoadTheme()
    {
        SideTextures[Side.Bottom] = new TextureInfo(Color.DarkGray, TextureKey.Concrete, TextureStyle.Tile, TileSize: 2.0f);
        MainTexture = new TextureInfo(Color.White, TextureKey.Plain);
    }
}
