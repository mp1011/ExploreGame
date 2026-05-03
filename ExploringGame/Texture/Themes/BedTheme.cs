using Microsoft.Xna.Framework;

namespace ExploringGame.Texture.Themes;

public class BedTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public BedTheme()
    {
        MainTexture = new TextureInfo(Color.Blue, TextureKey.Plain);
    }
}
