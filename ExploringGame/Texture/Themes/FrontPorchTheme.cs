using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture.Themes;

public class FrontPorchTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public FrontPorchTheme()
    {
        SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Wood, TextureStyle.Tile, TileSize: 2.0f);
        MainTexture = new TextureInfo(Color.White, TextureKey.Plain);
    }
}
