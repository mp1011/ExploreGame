using ExploringGame.LevelControl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Texture;

public enum TextureKey
{
    None = 0,
    Wood = 1,
    Wall = 2,
    Ceiling = 3,
    Floor = 4,
    Plain = 5,
    Brick = 6,
    Tile = 7,
    Sky = 8,
    Grass = 9,
    Siding = 10,
    Concrete = 11,
    Fence = 12

}

public enum TextureSheetKey
{
    Basement,
    Upstairs,
    Sky,
    Outdoors
}

public abstract class TextureSheet
{
    public abstract TextureSheetKey Key { get; }

    protected TextureSheet(ContentManager contentManager, string texture)
    {
        Texture = contentManager.Load<Texture2D>(texture);
    }

    // Protected constructor for testing - allows creating TextureSheet without loading actual textures
    protected TextureSheet()
    {
        Texture = null!;
    }

    public Texture2D Texture { get; }    
    public Dictionary<TextureKey, Rectangle> TextureLocations { get; } = new Dictionary<TextureKey, Rectangle>();


    protected TextureSheet Add(TextureKey key, int left, int top, int right, int bottom)
    {
        TextureLocations[key] = new Rectangle(left, top, (right-left), (bottom-top));
        return this;
    }

    public virtual Vector2 TexturePosition(TextureKey key, Vector2 position)
    {
        var textureLoc = TextureLocations[key];

        var pixelPosition = new Vector2(textureLoc.X + (position.X * textureLoc.Width),
                                   textureLoc.Y + (position.Y * textureLoc.Height));

        return pixelPosition / new Vector2(Texture.Width, Texture.Height);  
    }

}


public class BasementTextureSheet : TextureSheet
{
    public override TextureSheetKey Key => TextureSheetKey.Basement;

    public BasementTextureSheet(ContentManager content) : base(content, "basement")
    {
        Add(TextureKey.Floor, left: 1753, top: 886, right: 2866, bottom: 1640);
        Add(TextureKey.Wall, left: 2975, top: 808, right: 4483, bottom: 2806);
        Add(TextureKey.Ceiling, left: 214, top: 24, right: 1523, bottom: 2008);
        Add(TextureKey.Wood, left: 1995, top: 80, right: 3625, bottom: 669);
        Add(TextureKey.None, left: 912, top: 2221, right: 922, bottom: 2231);
    }
}


public class UpstairsTextureSheet : TextureSheet
{
    public override TextureSheetKey Key => TextureSheetKey.Upstairs;

    public UpstairsTextureSheet(ContentManager content) : base(content, "upstairs")
    {
        Add(TextureKey.Floor, left: 26, top: 24, right: 1078, bottom: 1358);
        Add(TextureKey.Plain, left: 4005, top: 1109, right: 4643, bottom: 1807);
        Add(TextureKey.Wood, left: 2276, top: 31, right: 3791, bottom: 1501);
        Add(TextureKey.Brick, left: 3830, top: 21, right: 4914, bottom: 976);
        Add(TextureKey.Tile, left: 1108, top: 37, right: 2231, bottom: 1169);
        Add(TextureKey.None, left: 100, top: 1500, right: 200, bottom: 1600);
    }
}

public class SkyTextureSheet : TextureSheet
{
    public override TextureSheetKey Key => TextureSheetKey.Sky;

    public SkyTextureSheet(ContentManager content) : base(content, "sky")
    {
        Add(TextureKey.Sky, left: 0, top: 0, right: 4096, bottom: 1347);
    }
}

public class OutdoorsTextureSheet : TextureSheet
{
    public override TextureSheetKey Key => TextureSheetKey.Outdoors;

    public OutdoorsTextureSheet(ContentManager content) : base(content, "outdoors")
    {
        Add(TextureKey.Grass, left: 299, top: 268, right: 1812, bottom: 1756);

        // Add(TextureKey.Siding, left: 316, top: 1889, right: 721, bottom: 2097);

        Add(TextureKey.Siding, left: 60, top: 2777, right: 1969, bottom: 3789);

        Add(TextureKey.Wood, left: 872, top: 1883, right: 1363, bottom: 2299);
        Add(TextureKey.Brick, left: 1549, top: 1881, right: 2352, bottom: 2704);
        Add(TextureKey.Plain, left: 2672, top: 2025, right: 3313, bottom: 2727);
        Add(TextureKey.Concrete, left: 2429, top: 408, right: 3762, bottom: 1803);
        Add(TextureKey.Fence, left: 3360, top: 2696, right: 4048, bottom: 4048);

        Add(TextureKey.None, left: 50, top: 50, right: 100, bottom: 100);
    }
}
