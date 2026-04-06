using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture.Themes;

public class FrontPorchTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public FrontPorchTheme()
    {
        SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Wood, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));
        SideTextures[Side.East] = new TextureInfo(TextureKey.Siding, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));

        MainTexture = new TextureInfo(Color.White, TextureKey.Plain);
    }
}
