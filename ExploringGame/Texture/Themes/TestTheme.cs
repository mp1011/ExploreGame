using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class TestTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Basement;

    public TestTheme()
    {
        MainTexture = new TextureInfo(Color.Pink, TextureKey.Ceiling);
        SideTextures[Side.Top] = new TextureInfo(Color.Gray, TextureKey.Ceiling);
        SideTextures[Side.Bottom] = new TextureInfo(Color.Purple, TextureKey.Ceiling);
    }
}
