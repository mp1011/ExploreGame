using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class KitchenTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public KitchenTheme()
    {
        SideTextures[Side.Top] = new TextureInfo(Color.White, TextureKey.Plain);
        SideTextures[Side.Bottom] = new TextureInfo(Color.White, TextureKey.Tile, TextureStyle.Tile, 
            new TilingInfo(TileSize: 2, TilingOrigin: new Vector3(-.1f,0f,0.5f)));
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Plain);
    }
}
