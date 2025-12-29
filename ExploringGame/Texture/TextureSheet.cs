using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Texture;

public enum TextureKey
{
    None = 0,
    Wood = 1,
    Wall = 2,
    Ceiling = 3,
    Floor = 4
}

public class TextureSheet
{
    public TextureSheet(Texture2D texture)
    {
        Texture = texture;
    }

    public Texture2D Texture { get; }    
    public Dictionary<TextureKey, Rectangle> TextureLocations { get; } = new Dictionary<TextureKey, Rectangle>();


    public TextureSheet Add(TextureKey key, int left, int top, int right, int bottom)
    {
        TextureLocations[key] = new Rectangle(left, top, (right-left), (bottom-top));
        return this;
    }

    public Vector2 TexturePosition(TextureKey key, Vector2 position)
    {
        var textureLoc = TextureLocations[key];

        var pixelPosition = new Vector2(textureLoc.X + (position.X * textureLoc.Width),
                                   textureLoc.Y + (position.Y * textureLoc.Height));
        
        return pixelPosition / new Vector2(Texture.Width, Texture.Height);  
    }

}
