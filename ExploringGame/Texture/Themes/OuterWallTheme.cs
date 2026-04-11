using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class OuterWallTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public OuterWallTheme()
    {
        MainTexture = new TextureInfo(Color.White, TextureKey.Siding, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));
    }
}
