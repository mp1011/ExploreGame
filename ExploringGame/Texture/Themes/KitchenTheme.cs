using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class KitchenTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public KitchenTheme()
    {
        SideTextures[Side.Top] = new TextureInfo(Color.White, TextureKey.Plain);
        SideTextures[Side.Bottom] = new TextureInfo(Color.White, TextureKey.Tile, TextureStyle.Tile, TileSize: 2);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Plain);
    }
}
