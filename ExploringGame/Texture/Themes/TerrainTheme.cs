using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class TerrainTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public TerrainTheme()
    {
        MainTexture = new TextureInfo(TextureKey.Grass);
    }
}
