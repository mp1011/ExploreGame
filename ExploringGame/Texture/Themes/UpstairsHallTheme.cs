using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class UpstairsHallTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Upstairs;

    public UpstairsHallTheme()
    {
        SideTextures[Side.Top] = new TextureInfo(Color.White, TextureKey.Plain);      
        SideTextures[Side.Bottom] = new TextureInfo(Color.Brown, TextureKey.Floor);
        MainTexture = new TextureInfo(Color.LightGray, TextureKey.Plain, TextureStyle.HorizontalRepeat, TileSize: 3.0f);
    }
}
