using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class BasementRoomTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Basement;

    public BasementRoomTheme()
    {
        SideTextures[Side.Top] = new TextureInfo(TextureKey.Ceiling);
        SideTextures[Side.Bottom] = new TextureInfo(TextureKey.Floor);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Wall);
    }
}
