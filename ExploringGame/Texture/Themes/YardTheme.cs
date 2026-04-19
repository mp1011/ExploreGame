using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public class YardTheme : Theme
{
    public override TextureSheetKey TextureSheetKey => TextureSheetKey.Outdoors;

    public YardTheme()
    {
        SideTextures[Side.Bottom] = new TextureInfo(Color.White, TextureKey.Grass, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));
        MainTexture = new TextureInfo(Color.White, TextureKey.Plain);

        AdditionalTextures[TextureKey.Concrete] = new TextureInfo(Color.White, TextureKey.Concrete, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));
        AdditionalTextures[TextureKey.Siding] = new TextureInfo(Color.White, TextureKey.Siding, TextureStyle.Tile, new TilingInfo(TileSize: 2.0f));
        AdditionalTextures[TextureKey.Brick] = new TextureInfo(Color.White, TextureKey.Brick, TextureStyle.Tile, new TilingInfo(TileSize: 1.0f));
    }
}
