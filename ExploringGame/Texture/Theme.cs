using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Texture;

public class Theme
{
    public TextureInfo MainTexture { get; set; } = new TextureInfo(Color.Magenta);
    public Dictionary<Side, TextureInfo> SideTextures { get; set; } = new();

    public virtual TextureSheetKey TextureSheetKey { get; } = TextureSheetKey.Basement;

    public Theme() { }

    public Theme(TextureSheetKey key)
    {
        TextureSheetKey = key;
    }

    public Theme(TextureKey key)
    {
        MainTexture = new TextureInfo(Key: key);
    }

    public Theme(Color color)
    {
        MainTexture = new TextureInfo(Color: color);
    }

    public void CopyFrom(Theme other)
    {
        MainTexture = other.MainTexture;
        SideTextures = new Dictionary<Side, TextureInfo>(other.SideTextures);
    }   

    public TextureInfo GetTexture(TextureKey key)
    {
        if (MainTexture.Key == key)
            return MainTexture;
        else
            return SideTextures.Values.FirstOrDefault(p => p.Key == key) ?? throw new System.Exception($"No texture in this theme has key {key}");
    }

    public TextureInfo TextureInfoForSide(Side side)
    {
        if (SideTextures.TryGetValue(side, out var texture))
            return texture;
        else
            return MainTexture;
    }

    public static Theme Missing { get => new Theme { MainTexture = new TextureInfo(Color.Magenta) }; }
}
