using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class BasicFurnitureTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Basement;

    public BasicFurnitureTheme() : this(Color.Tan) { }
    
    public BasicFurnitureTheme(Color color)
    {
        MainTexture = new TextureInfo(color, TextureKey.Wood);
    }
}
