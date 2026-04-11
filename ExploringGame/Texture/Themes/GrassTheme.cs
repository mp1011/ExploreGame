using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class GrassTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public GrassTheme()
    {
        MainTexture = new TextureInfo(new Color(100, 140, 70), TextureKey.Plain);
    }
}
