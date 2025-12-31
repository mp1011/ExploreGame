using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public enum TextureStyle
{
    FillSide,
    XZTile,
}

public record TextureInfo(Color Color, TextureKey Key, TextureStyle Style = TextureStyle.FillSide, float? TileSize = null)
{
    public TextureInfo(TextureKey Key, TextureStyle Style = TextureStyle.FillSide, float? TileSize = null) 
        : this(Color.White, Key, Style, TileSize) { }

    public TextureInfo(Color Color) : this(Color, TextureKey.None) { }

}
