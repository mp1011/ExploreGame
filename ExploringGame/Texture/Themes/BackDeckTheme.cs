using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture.Themes;

public class BackDeckTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public BackDeckTheme()
    {
        SideTextures[Side.Bottom] = new TextureInfo(Color.DarkRed, TextureKey.Wood, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));
        SideTextures[Side.Top] = new TextureInfo(Color.DarkRed, TextureKey.Wood, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));

        MainTexture = new TextureInfo(Color.Red, TextureKey.Plain);
    }
}
