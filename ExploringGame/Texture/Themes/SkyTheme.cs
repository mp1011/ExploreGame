namespace ExploringGame.Texture;

public class SkyTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Sky;

    public SkyTheme()
    {
        MainTexture = new TextureInfo(TextureKey.Sky, TextureStyle.Spherical);
    }
}
