using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class FenceTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public FenceTheme()
    {
        MainTexture = new TextureInfo(Color.White, TextureKey.Fence, TextureStyle.HorizontalRepeat, new TilingInfo(TileSize: 2.0f));
    }
}
