using Microsoft.Xna.Framework;

namespace ExploringGame.Texture.Themes;

public class PillowTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public PillowTheme()
    {
        MainTexture = new TextureInfo(Color.LightBlue, TextureKey.Plain);
    }
}
