using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public enum TextureStyle
{
    FillSide,
    Tile,
    HorizontalRepeat,
    Spherical
}

public static class TextureStyleExtensions
{
    public static bool HasTiling(this TextureStyle style) => style switch
    {
        TextureStyle.Tile => true,
        TextureStyle.HorizontalRepeat => true,
        _ => false
    };
}

public record TilingInfo(float TileSize, Vector3? TilingOrigin = null)
{
    public Vector3 GetTilingOrigin() => TilingOrigin ?? Vector3.Zero;
}

public record TextureInfo(Color Color, TextureKey Key, TextureStyle Style = TextureStyle.FillSide, TilingInfo? TilingInfo = null)
{
    public TextureInfo(TextureKey Key, TextureStyle Style = TextureStyle.FillSide, TilingInfo? TilingInfo = null) 
        : this(Color.White, Key, Style, TilingInfo) { }

    public TextureInfo(Color Color) : this(Color, TextureKey.None) { }

}
