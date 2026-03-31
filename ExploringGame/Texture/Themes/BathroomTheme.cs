using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class BathroomTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public BathroomTheme()
    {
        MainTexture = new TextureInfo(Color.White, TextureKey.Plain);
    }
}
