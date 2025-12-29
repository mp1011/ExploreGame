using Microsoft.Xna.Framework;

namespace ExploringGame.Texture;

public record TextureInfo(Color Color, TextureKey Key)
{
    public TextureInfo(TextureKey Key) : this(Color.White, Key) { }

    public TextureInfo(Color Color) : this(Color, TextureKey.None) { }

}
