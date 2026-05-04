using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class BasicFurnitureTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Basement;

    public BasicFurnitureTheme()
    {
        MainTexture = new TextureInfo(Color.Tan, TextureKey.Wood);
    }
}
