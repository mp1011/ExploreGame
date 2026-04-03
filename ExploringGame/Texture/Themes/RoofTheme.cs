using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class RoofTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public RoofTheme()
    {
        MainTexture = new TextureInfo(Color.DarkGray, TextureKey.Plain, TextureStyle.Tile, TileSize: 1.0f);
    }
}
